using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using Stripe;

namespace PEAS.Services
{
    public interface IPaymentService
    {
        string StartPayment(Guid orderId, int tip);
    }

    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PaymentService> _logger;

        private readonly string tipMetadata = "tip";

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
                
                var paymentIntentService = new PaymentIntentService();

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
                            {tipMetadata, $"{tip}"},
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
                        Metadata = new Dictionary<string, string>
                        {
                            {"orderId", $"{order.Id}"},
                            {tipMetadata, $"{tip}"}
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

        private int getPaymentIntentAmount(Order order, int tip)
        {
            return order.Price + tip;
        }
    }
}