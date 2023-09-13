﻿namespace PEAS.Models.Business
{
    public class UpdateBlock
    {
        public Guid Id { get; set; }

        public Uri? Image { get; set; }

        public decimal? Price { get; set; }

        public int? Duration { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}