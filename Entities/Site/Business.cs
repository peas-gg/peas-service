using PEAS.Entities.Authentication;
using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Site
{
    public class Business
    {
        [Key]
        public int Id { get; set; }

        public required Account Account { get; set; }

        [StringLength(16)]
        public required string Sign { get; set; }

        [StringLength(30)]
        public required string Name { get; set; }

        public string? Category { get; set; }

        public required string Color { get; set; }

        public required string Description { get; set; }

        public required Uri ProfilePhoto { get; set; }

        public string? TwitterLink { get; set; }

        public string? InstagramLink { get; set; }

        public string? TikTokLink { get; set; }

        public required string Location { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public required bool IsActive { get; set; }

        public required DateTime Created { get; set; }

        public DateTime? Deactivated { get; set; }
    }
}