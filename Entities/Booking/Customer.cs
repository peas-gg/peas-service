using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Booking
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string Phone { get; set; }
    }
}