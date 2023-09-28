using System;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;

namespace PEAS.Models.Business.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }

        public required Customer Customer { get; set; }

        public required Currency Currency { get; set; }

        public required Entities.Booking.Order.Status OrderStatus { get; set; }

        public required int Price { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required Uri Image { get; set; }

        public string? Note { get; set; }

        public bool DidRequestPayment { get; set; }

        public Payment? Payment { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }

        public required DateTime Created { get; set; }
    }
}