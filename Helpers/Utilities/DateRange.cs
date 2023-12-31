using System;
namespace PEAS.Helpers.Utilities
{
    public static partial class DateTimeExtensions
    {
        public static DateTime ResetTimeToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    public interface IRange<T>
    {
        T Start { get; }
        T End { get; }
        bool WithInRange(T value);
        bool WithInRange(IRange<T> range);
    }

    public class DateRange : IRange<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            if (End < Start)
            {
                throw new Exception("Invalid date range");
            }
        }

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }

        public bool WithInRange(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool WithInRange(IRange<DateTime> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }

        public static List<DateRange> GetAvailability(DateRange schedule, TimeSpan orderDuration, List<DateRange> existingOrders, List<DateRange> blockedTimeSlots)
        {
            DateTime prevDate = schedule.Start;
            DateTime currentDate = schedule.Start;
            List<DateRange> availability = new();

            currentDate += orderDuration;

            while (currentDate <= schedule.End)
            {
                var timeSlot = new DateRange(prevDate, currentDate);

                //Check if there is an order in the timeslot
                DateRange? existingOrderConflict = existingOrders.FirstOrDefault(x => x.WithInRange(timeSlot));
                if (existingOrderConflict != null)
                {
                    prevDate = existingOrderConflict.End;
                    currentDate = existingOrderConflict.End + orderDuration;
                    continue;
                }

                //Check if there is a blocked time in the timeslot
                DateRange? blockedTimeConflict = blockedTimeSlots.FirstOrDefault(x => x.WithInRange(timeSlot));
                if (blockedTimeConflict != null)
                {
                    prevDate = blockedTimeConflict.End;
                    currentDate = blockedTimeConflict.End + orderDuration;
                    continue;
                }

                //Try to add the timeslot since there are no conflicts
                if (timeSlot.Start > DateTime.UtcNow && timeSlot.End <= schedule.End)
                {
                    availability.Add(timeSlot);
                }
                prevDate = currentDate;
                currentDate += orderDuration;
            }
            return availability;
        }
    }
}