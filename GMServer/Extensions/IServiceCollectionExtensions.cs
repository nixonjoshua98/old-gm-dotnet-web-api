using GMServer.Authentication;
using GMServer.Cache;
using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Models.Settings;
using GMServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GMServer
{
    public static class IServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<IDataFileCache, DataFileCache>();
            services.AddSingleton<ArtefactsService>();
            services.AddSingleton<UnitService>();
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
            services.AddSingleton(new ServerRefresh<IDailyServerRefresh>() { Hour = 20, Interval = TimeSpan.FromDays(1) });
        }

        public static void AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            IMongoClient client = new MongoClient(configuration["MongoSettings:ConnectionString"]);

            IMongoDatabase database = client.GetDatabase(configuration["MongoSettings:DatabaseName"]);

            services.AddSingleton(database);
            services.AddSingleton(client);
        }

        public static T Configure<T>(this IServiceCollection services, IConfiguration configuration, string section)
        {
            T model = configuration.GetSection(section).Get<T>();

            services.AddSingleton(typeof(T), model);

            return model;
        }

        public static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JWTBearerSettings settings = services.Configure<JWTBearerSettings>(configuration, "AuthenticationSettings");

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                // Setup our JWT authentication handler
                opt.SecurityTokenValidators.Clear();
                opt.SecurityTokenValidators.Add(new JWTSecurityTokenHandler(services.BuildServiceProvider()));

                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Key)),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience
                };

                opt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.OnStarting(() =>
                        {

                            // Our JWT handler may throw a custom exception which will 'hopefully' force the user to
                            // invalidate and delete all local game progress.
                            if (context.Exception.GetType() == typeof(InvalidTokenException))
                            {
                                // Client should read this header and then invalidate itself
                                context.Response.Headers.Add("Invalid-Token", "true");
                            }

                            return Task.CompletedTask;
                        });

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
