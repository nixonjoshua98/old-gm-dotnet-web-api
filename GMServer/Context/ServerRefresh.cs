using System;

namespace GMServer.Context
{
    public record CurrentServerRefresh<T>(DateTime Previous, DateTime Next);

    public interface IDailyServerRefresh { }

    public class ServerRefresh<T>
    {
        public TimeSpan Interval;

        public int WeekDay = -1;
        public int MonthDate = -1;

        public int Hour;
        public int Minute;
        public int Second = 0;

        public CurrentServerRefresh<T> Current => RefreshPairFromDate(DateTime.UtcNow);

        CurrentServerRefresh<T> RefreshPairFromDate(DateTime dt)
        {
            DateTime refreshTime = new DateTime(dt.Year, dt.Month, dt.Day, Hour, Minute, Second);

            // Week Day (Mon, Tue etc.)
            if (WeekDay > -1)
            {
                while (refreshTime.DayOfWeek != (DayOfWeek)WeekDay)
                    refreshTime -= TimeSpan.FromDays(1);

                return new(refreshTime, refreshTime + TimeSpan.FromDays(7));
            }

            // Month date
            if (MonthDate > -1)
            {
                while (refreshTime.Day != MonthDate)
                    refreshTime -= TimeSpan.FromDays(1);

                DateTime nextRefresh = new DateTime(refreshTime.Year, refreshTime.Month + 1, refreshTime.Day, refreshTime.Hour, refreshTime.Minute, refreshTime.Second);

                return new(refreshTime, nextRefresh);
            }

            // General Interval (1 hour, 3 days etc)
            if (refreshTime > dt)
            {
                while (refreshTime > dt)
                    refreshTime -= Interval;

                return new(refreshTime, refreshTime + Interval);
            }
            else
            {
                while (dt > refreshTime)
                    refreshTime += Interval;

                return new(refreshTime - Interval, refreshTime);
            }
        }
    }
}