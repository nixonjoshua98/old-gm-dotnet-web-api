namespace GMServer.Context
{
    public class RequestContext
    {
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;

        public RequestContext (ServerRefresh<IDailyServerRefresh> daily)
        {
            DailyRefresh = daily.Current;
        }
    }
}
