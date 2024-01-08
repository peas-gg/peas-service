using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PEAS.Entities.Site;

namespace PEAS.Entities.Booking
{
    [Owned]
    public class TimeBlock
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required string Title { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }
    }
}