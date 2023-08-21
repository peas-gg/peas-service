using System;
namespace PEAS.Models.Business
{
    public class UpdateBusiness
    {
        public Guid Id { get; set; }

        public string? Sign { get; set; }

        public string? Name { get; set; }

        public string? Color { get; set; }

        public string? Description { get; set; }

        public Uri? ProfilePhoto { get; set; }

        public string? Twitter { get; set; }

        public string? Instagram { get; set; }

        public string? Tiktok { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}