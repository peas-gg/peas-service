using System;
namespace PEAS.Models.Business.Order
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public int Price { get; set; }
    }
}