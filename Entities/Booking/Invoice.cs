using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Booking
{
    public class Invoice
    {
        public int Id { get; set; }

        public required Order Order { get; set; }

        public List<Payment>? Payments { get; set; }

        [Timestamp]
        public required byte[] Version { get; set; }
    }
}