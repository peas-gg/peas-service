using Microsoft.EntityFrameworkCore;
using PEAS.Entities.Site;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Booking
{
    [Owned]
    public class Schedule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required DayOfWeek DayOfWeek { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }
    }
}