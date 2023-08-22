namespace PEAS.Models.Business
{
    public class TemplateResponse
    {
        public Guid Id { get; set; }

        public required string Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo { get; set; }

        public required BusinessResponse Business { get; set; }
    }
}