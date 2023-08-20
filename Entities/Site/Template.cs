using System.ComponentModel.DataAnnotations;

namespace PEAS.Entities.Site
{
    public class Template
    {
        [Key]
        public int Id { get; set; }

        public required string Category { get; set; }

        public string? Details { get; set; }

        public required Uri Photo  { get; set; }

        public required Business Business { get; set; }
    }
}