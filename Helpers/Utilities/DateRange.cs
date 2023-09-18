using System;
namespace PEAS.Helpers.Utilities
{
    public static partial class DateTimeExtensions
    {
        public static DateTime ResetTimeToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }
        public static DateTime ResetTimeToEndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(1).AddTicks(-1);
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

        public static List<DateRange> GetAvailability(DateRange schedule, TimeSpan orderDuration, List<DateRange> existingOrders)
        {
            DateTime prevDate = schedule.Start;
            DateTime currentDate = schedule.Start;
            List<DateRange> availability = new();
            while (currentDate < schedule.End)
            {
                currentDate += orderDuration;
                var timeSlot = new DateRange(prevDate, currentDate);
                if (!existingOrders.Any(x => x.WithInRange(timeSlot)))
                {
                    availability.Add(timeSlot);
                }
                prevDate = currentDate;
            }
            return availability;
        }
    }
}