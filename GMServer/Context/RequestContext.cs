namespace GMServer.Context
{
    public class RequestContext
    {
        public CurrentServerRefresh DailyRefresh;

        public RequestContext (ServerRefresh<IDailyServerRefresh> daily)
        {
            DailyRefresh = daily.Current;
        }
    }
}
