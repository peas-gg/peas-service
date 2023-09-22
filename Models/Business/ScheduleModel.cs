using System;
namespace PEAS.Models.Business
{
    public class ScheduleModel
    {
        public Guid Id { get; set; }
        public required DayOfWeek DayOfWeek { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }
    }
}