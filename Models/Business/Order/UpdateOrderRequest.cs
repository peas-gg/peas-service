namespace PEAS.Models.Business.Order
{
    public class UpdateOrderRequest
    {
        public Guid OrderId { get; set; }
        public Entities.Booking.Order.Status OrderStatus { get; set; }
    }
}