using PEAS.Entities.Site;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEAS.Entities.Booking
{
    public class Schedule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public required Business Business { get; set; }

        public required int DayOfWeek { get; set; }

        public required int StartTime { get; set; }

        public required int EndTime { get; set; }
    }
}