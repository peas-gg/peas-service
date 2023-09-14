using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Site
{
    public class Business
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Account Account { get; set; }

        [StringLength(16)]
        public required string Sign { get; set; }

        [StringLength(30)]
        public required string Name { get; set; }

        public string? Category { get; set; }

        public required Currency Currency { get; set; }

        public required string Color { get; set; }

        public required string Description { get; set; }

        public required Uri ProfilePhoto { get; set; }

        public string? Twitter { get; set; }

        public string? Instagram { get; set; }

        public string? Tiktok { get; set; }

        public required string Location { get; set; }

        public required string TimeZone { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public required bool IsActive { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Deactivated { get; set; }

        public required List<Block> Blocks { get; set; }

        public required List<Schedule>? Schedules { get; set; }
    }
}