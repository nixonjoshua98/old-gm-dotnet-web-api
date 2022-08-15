using Microsoft.Extensions.DependencyInjection;
using SRC.DataFiles.Cache;

namespace GMServer.Caching.DataFiles.Extensions
{
    public static class DataFileExtensions
    {
        public static void AddDataFileCache(this IServiceCollection services)
        {
            services.AddSingleton<IDataFileCache, DataFileCache>();
        }
    }
}
