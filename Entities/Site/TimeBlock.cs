using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Site
{
    public class TimeBlock
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required string Title { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }

        public required DateTime Created { get; set; }

        public required DateTime LastUpdated { get; set; }
    }
}