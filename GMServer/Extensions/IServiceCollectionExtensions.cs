using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SRC.Context;
using SRC.Services;
using System;

namespace SRC
{
    public static class IServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ArtefactsService>();
            services.AddSingleton<MercsService>();
            services.AddSingleton<ArmouryService>();
            services.AddSingleton<AccountStatsService>();
            services.AddSingleton<IBountiesService, BountiesService>();
            services.AddSingleton<QuestsService>();
            services.AddSingleton<PrestigeService>();
            services.AddSingleton<CurrenciesService>();
            services.AddSingleton<BountyShopService>();

            // Scoped
            services.AddScoped<RequestContext>();

            // Server Refresh Intervals
            services.AddSingleton(new ServerRefresh<IDailyRefresh>() { Hour = 20, Interval = TimeSpan.FromDays(1) });
        }

        public static void Configure<T>(this IServiceCollection services, IConfiguration configuration, string section)
        {
            T model = configuration.GetSection(section).Get<T>();

            services.AddSingleton(typeof(T), model);
        }
    }
}
