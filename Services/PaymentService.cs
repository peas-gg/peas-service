using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Models;
using Stripe;

namespace PEAS.Services
{
    public interface IPaymentService
    {
        string StartPayment(Guid orderId, int tip);
        EmptyResponse CompletePayment(Guid orderId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PaymentService> _logger;

        private readonly string tipMetadataKey = "tip";
        private readonly decimal platformFeeRate = 0.08M;

        public PaymentService(IConfiguration configuration, DataContext context, IPushNotificationService pushNotificationService, ILogger<PaymentService> logger)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
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
                            {"orderId", $"{order.Id}"},
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

        public EmptyResponse CompletePayment(Guid orderId)
        {
            try
            {
                Order? order = _context.Orders
                    .Include(x => x.Payment)
                    .Include(x => x.Business)
                    .ThenInclude(x => x.Account)
                    .First(x => x.Id == orderId);

                if (order == null || order.Payment == null)
                {
                    throw new AppException("Invalid OrderId");
                }

                PaymentIntentService paymentIntentService = new PaymentIntentService();

                PaymentIntent paymentIntent = paymentIntentService.Get(order.Payment.PaymentIntentId);
                switch (paymentIntent.Status)
                {
                    case "requires_payment_method":
                    case "requires_confirmation":
                    case "requires_action":
                    case "processing":
                    case "requires_capture":
                    case "canceled":
                    default:
                        throw new AppException("Payment could not be confirmed. Please try that again.");
                    case "succeeded":
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

                        _context.Update(order);
                        _context.SaveChanges();

                        _pushNotificationService.SendPaymentReceivedPush(order.Business.Account, order);
                        return new EmptyResponse();
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
    }
}