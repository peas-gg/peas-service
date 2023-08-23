using System;
namespace PEAS.Models.Business
{
    public class UpdateBlock
    {
        public Guid Id { get; set; }

        public Uri? Image { get; set; }

        public double? Price { get; set; }

        public int? Duration { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}