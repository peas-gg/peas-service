using PEAS.Entities.Site;

namespace PEAS.Models.Business
{
    public class CreateBusiness
    {
        public required string Sign { get; set; }

        public required string Name { get; set; }

        public Category? Category { get; set; }

        public required string Color { get; set; }

        public required string Description { get; set; }

        public required Uri ProfilePhoto { get; set; }

        public string? Twitter { get; set; }

        public string? Instagram { get; set; }

        public string? Tiktok { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public required List<Block> Blocks { get; set; }
    }
}