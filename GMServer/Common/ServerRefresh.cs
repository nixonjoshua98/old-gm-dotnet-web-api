using System;

namespace GMServer.Context
{
    public class CurrentServerRefresh<T>
    {
        public DateTime Previous { get; set; }
        public DateTime Next { get; set; }

        public CurrentServerRefresh(DateTime prev, DateTime next)
        {
            Previous = prev;
            Next = next;
        }

        public bool IsBetween(DateTime dt) => Previous < dt && Next > dt;
    }

    public interface IDailyServerRefresh { }

    public class ServerRefresh<T>
    {
        public TimeSpan Interval;

        public int WeekDay = -1;
        public int MonthDate = -1;

        public int Hour;
        public int Minute;
        public int Second;

        public CurrentServerRefresh<T> Current => RefreshPairFromDate(DateTime.UtcNow);

        private CurrentServerRefresh<T> RefreshPairFromDate(DateTime dt)
        {
            DateTime refreshTime = new DateTime(dt.Year, dt.Month, dt.Day, Hour, Minute, Second, DateTimeKind.Utc);

            if (WeekDay > -1)
                return WeeklyInterval(refreshTime);

            else if (MonthDate > -1)
                return MonhlyInterval(refreshTime);

            return GeneralInterval(refreshTime, dt);
        }

        private CurrentServerRefresh<T> WeeklyInterval(DateTime refreshTime)
        {
            while (refreshTime.DayOfWeek != (DayOfWeek)WeekDay)
                refreshTime -= TimeSpan.FromDays(1);

            return new(refreshTime, refreshTime + TimeSpan.FromDays(7));
        }

        private CurrentServerRefresh<T> MonhlyInterval(DateTime refreshTime)
        {
            while (refreshTime.Day != MonthDate)
                refreshTime -= TimeSpan.FromDays(1);

            DateTime nextRefresh = new(refreshTime.Year, refreshTime.Month + 1, refreshTime.Day, refreshTime.Hour, refreshTime.Minute, refreshTime.Second);

            return new(refreshTime, nextRefresh);
        }

        private CurrentServerRefresh<T> GeneralInterval(DateTime refreshTime, DateTime now)
        {
            if (refreshTime > now)
            {
                while (refreshTime > now)
                    refreshTime -= Interval;

                return new(refreshTime, refreshTime + Interval);
            }
            else
            {
                while (now > refreshTime)
                    refreshTime += Interval;

                return new(refreshTime - Interval, refreshTime);
            }
        }
    }
}