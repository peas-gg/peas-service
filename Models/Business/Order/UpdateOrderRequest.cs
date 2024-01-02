using PEAS.Helpers.Utilities;

namespace PEAS.Models.Business.Order
{
    public class UpdateOrderRequest
    {
        public Guid OrderId { get; set; }
        public Entities.Booking.Order.Status? OrderStatus { get; set; }
        public DateRange? DateRange { get; set; }
    }
}