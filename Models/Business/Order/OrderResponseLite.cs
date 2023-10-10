using System;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;

namespace PEAS.Models.Business.Order
{
    public class OrderResponseLite
    {
        public required string BusinessSign { get; set; }

        public required Uri BusinessProfilePhoto { get; set; }

        public required Currency Currency { get; set; }

        public required int Price { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required Uri Image { get; set; }

        public required DateTime StartTime { get; set; }
    }
}