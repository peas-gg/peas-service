using System;

namespace PEAS.Models.Business
{
    public class CreateBusiness
    {
        public required string Sign { get; set; }

        public required string Name { get; set; }

        public string? Category { get; set; }

        public required string Color { get; set; }

        public required string Description { get; set; }

        public required Uri ProfilePhoto { get; set; }

        public string? TwitterLink { get; set; }

        public string? InstagramLink { get; set; }

        public string? TikTokLink { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }
    }
}