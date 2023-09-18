using System;
namespace PEAS.Models.Business.Schedule
{
    public class ScheduleRequest
    {
        public required DayOfWeek DayOfWeek { get; set; }

        public required DateTime StartTime { get; set; }

        public required DateTime EndTime { get; set; }
    }
}