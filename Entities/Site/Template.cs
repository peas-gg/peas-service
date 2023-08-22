using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Site
{
    public class Template
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required string Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo  { get; set; }

        public required Business Business { get; set; }
    }
}