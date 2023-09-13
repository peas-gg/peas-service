using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required Block Block { get; set; }

        public required Customer Customer { get; set; }

        public required string Title { get; set; }

        public required string Detail { get; set; }

        public required Currency Currency { get; set; }

        public required decimal Price { get; set; }

        public required Uri Image { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }

        public required OrderStatus Status  { get; set; }

        public string? Note  { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Paid { get; set; }

        public List<Payment>? Payments { get; set; }

        [Timestamp]
        public required byte[] Version { get; set; }
    }
}