using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Entities.Booking;
using PEAS.Helpers;

namespace PEAS.Services
{
    public interface IPaymentService
    {
        string RequestPayment(Guid orderId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly DataContext _context;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(DataContext context, IPushNotificationService pushNotificationService, ILogger<PaymentService> logger)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        public string RequestPayment(Guid orderId)
        {
            try
            {
                Order? order = _context.Orders.Include(x => x.Payment).First(x => x.Id == orderId);
                if (order == null || !order.DidRequestPayment)
                {
                    throw new AppException("Invalid OrderId");
                }

                if (order.Payment?.Total >= order.Price)
                {
                    throw new AppException("Order has been paid for");
                }

                if (order.Payment != null)
                {
                    //Return PaymentIntent if it exists
                    return order.Payment.PaymentIntentId;
                }
                else
                {
                    //Create PaymentIntent if it does not exist
                    return "";
                }
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }
    }
}