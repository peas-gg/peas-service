﻿namespace PEAS.Models.Business
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

        public required string TimeZone { get; set; }

        public required bool IsActive { get; set; }
    }
}