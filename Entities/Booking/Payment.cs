using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    [Owned]
    public class Payment
    {
        public int Id { get; set; }

        public Currency Currency { get; set; }

        public required decimal Base { get; set; }

        public required decimal Tip { get; set; }

        public required decimal Fee { get; set; }

        public required decimal Total { get; set; }

        public required DateTime Created { get; set; }
    }
}