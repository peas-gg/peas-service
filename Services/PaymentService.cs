using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Hubs;
using PEAS.Models;
using Stripe;

namespace PEAS.Services
{
    public interface IPaymentService
    {
        string StartPayment(Guid orderId, int tip);
        Task<EmptyResponse> CompletePayment(HttpRequest httpRequest);
    }

    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;
        private readonly IHubContext<AppHub> _hubContext;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PaymentService> _logger;

        private readonly string stripeWebHookKey;

        private readonly string tipMetadataKey = "tip";
        private readonly string orderIdMetadataKey = "orderId";
        private readonly decimal platformFeeRate = 0.08M;

        public PaymentService(
            IConfiguration configuration,
            DataContext context,
            IHubContext<AppHub> hubContext,
            IPushNotificationService pushNotificationService,
            ILogger<PaymentService> logger
            )
        {
            _context = context;
            _hubContext = hubContext;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
            stripeWebHookKey = configuration.GetSection("StripeWebHook").Value ?? "";
            StripeConfiguration.ApiKey = configuration.GetSection("Stripe").Value ?? "";

        }

        public string StartPayment(Guid orderId, int tip)
        {
            try
            {
                Order? order = _context.Orders.Include(x => x.Payment).First(x => x.Id == orderId);

                if (order == null || order.OrderStatus == Order.Status.Declined)
                {
                    throw new AppException("Invalid OrderId");
                }

                if (!order.DidRequestPayment)
                {
                    throw new AppException("Payment has not been requested for this order");
                }
                
                PaymentIntentService paymentIntentService = new PaymentIntentService();

                if (order.Payment != null)
                {
                    if (order.Payment.Total >= order.Price)
                    {
                        throw new AppException("Order has been paid for");
                    }

                    //Update the payment intent to match the latest order price then return the client secret
                    var options = new PaymentIntentUpdateOptions
                    {
                        Amount = getPaymentIntentAmount(order, tip),
                        Metadata = new Dictionary<string, string>
                        {
                            {tipMetadataKey, $"{tip}"},
                        },
                    };

                    PaymentIntent paymentIntent = paymentIntentService.Update(order.Payment.PaymentIntentId, options);
                    return paymentIntent.ClientSecret;
                }
                else
                {
                    //Create PaymentIntent if it does not exist
                    var paymentIntentOptions = new PaymentIntentCreateOptions
                    {
                        Amount = getPaymentIntentAmount(order, tip),
                        Currency = Currency.CAD.ToString().ToLower(),
                        SetupFutureUsage = "on_session",
                        Metadata = new Dictionary<string, string>
                        {
                            {orderIdMetadataKey, $"{order.Id}"},
                            {tipMetadataKey, $"{tip}"}
                        },
                        AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                        {
                            Enabled = true,
                        },
                    };

                    PaymentIntent paymentIntent = paymentIntentService.Create(paymentIntentOptions);

                    //Update Order
                    order.Payment = new Payment
                    {
                        PaymentIntentId = paymentIntent.Id,
                        Base = 0,
                        Deposit = 0,
                        Tip = 0,
                        Fee = 0,
                        Total = 0,
                        Created = DateTime.UtcNow
                    };

                    order.LastUpdated = DateTime.UtcNow;

                    _context.Update(order);
                    _context.SaveChanges();

                    return paymentIntent.ClientSecret;
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public async Task<EmptyResponse> CompletePayment(HttpRequest httpRequest)
        {
            try
            {
                var json = await new StreamReader(httpRequest.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(json,
                    httpRequest.Headers["Stripe-Signature"], stripeWebHookKey);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                    {
                        throw new AppException("Invalid Payment Intent object");
                    }

                    Guid orderId = Guid.Parse(paymentIntent!.Metadata[orderIdMetadataKey]);
                    Order? order = _context.Orders
                        .Include(x => x.Customer)
                        .Include(x => x.Payment)
                        .Include(x => x.Business)
                        .ThenInclude(x => x.Account)
                        .ThenInclude(x => x.Devices)
                        .First(x => x.Id == orderId);

                    if (order == null || order.Payment == null || order.Payment.Completed != null)
                    {
                        throw new AppException("Invalid OrderId");
                    }

                    //Update the order
                    int receviedAmount = (int)paymentIntent.AmountReceived;
                    string? tipString = paymentIntent.Metadata.GetValueOrDefault(tipMetadataKey);
                    int tipAmount = int.Parse(tipString!);
                    int baseAmount = receviedAmount - tipAmount;
                    int platformFee = (int)Math.Ceiling(baseAmount * platformFeeRate);

                    order.Payment.Base = baseAmount;
                    order.Payment.Tip = tipAmount;
                    order.Payment.Fee = platformFee;
                    order.Payment.Total = baseAmount - platformFee + tipAmount;
                    order.Payment.Completed = DateTime.UtcNow;

                    order.LastUpdated = DateTime.UtcNow;

                    _context.Update(order);
                    _context.SaveChanges();

                    //Send to Hub
                    sendPaymentConfirmationToAccount(order);

                    //Send Push Notification to the business owner
                    _pushNotificationService.SendPaymentReceivedPush(order.Business.Account, order, getPay(order));

                    return new EmptyResponse();
                }
                else
                {
                    throw new AppException("Payment not Successful");
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        private int getPaymentIntentAmount(Order order, int tip)
        {
            return order.Price + tip;
        }

        private async void sendPaymentConfirmationToAccount(Order order)
        {
            try
            {
                string message = $"Payment received from {order.Customer.FirstName} {order.Customer.LastName} {getPay(order)}";
                await _hubContext.Clients.Group(order.Business.Account.Id.ToString()).SendAsync("PaymentReceived", message);
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private string getPay(Order order)
        {
            int totalPaid = order.Payment!.Base + order.Payment!.Deposit + order.Payment.Tip;
            return $"(${Helpers.Utilities.Price.Format(totalPaid)})";
        }
    }
}