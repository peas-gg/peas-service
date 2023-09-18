using PEAS.Entities.Site;

namespace PEAS.Models.Business
{
    public class TemplateResponse
    {
        public Guid Id { get; set; }

        public required Category Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo { get; set; }

        public required BusinessResponse Business { get; set; }
    }
}