using PEAS.Entities.Site;

namespace PEAS.Models.Business
{
    public class BusinessResponse
    {
        public Guid Id { get; set; }

        public required string Sign { get; set; }

        public required string Name { get; set; }

        public string? Category { get; set; }

        public required string Color { get; set; }

        public required string Description { get; set; }

        public required Uri ProfilePhoto { get; set; }

        public string? Twitter { get; set; }

        public string? Instagram { get; set; }

        public string? Tiktok { get; set; }

        public required string Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public required string TimeZone { get; set; }

        public required bool IsActive { get; set; }

        public required List<Block> Blocks { get; set; }
    }
}