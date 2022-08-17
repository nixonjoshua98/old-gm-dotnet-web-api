using SRC.Context;

namespace SRC.Services
{
    public class RequestContext
    {
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;

        public RequestContext(ServerRefresh<IDailyRefresh> daily)
        {
            DailyRefresh = daily.Current;
        }
    }
}
