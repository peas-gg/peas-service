using PEAS.Entities.Site;

namespace PEAS.Models.Business
{
    public class CreateTemplate
    {
        public required Category Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo { get; set; }

        public required Guid BusinessId { get; set; }
    }
}