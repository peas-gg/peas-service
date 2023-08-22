using System;
namespace PEAS.Models.Business
{
    public class CreateTemplate
    {
        public required string Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo { get; set; }

        public required Guid BusinessId { get; set; }
    }
}