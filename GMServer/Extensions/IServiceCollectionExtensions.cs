using GMServer.Authentication;
using GMServer.Context;
using GMServer.Exceptions;
using GMServer.UserModels;
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
            services.AddSingleton<MercService>();
            services.AddSingleton<ArmouryService>();
            services.AddSingleton<AccountStatsService>();
            services.AddSingleton<BountiesService>();
            services.AddSingleton<QuestsService>();
            services.AddSingleton<CurrenciesService>();
            services.AddSingleton<BountyShopService>();

            // Scoped
            services.AddScoped<RequestContext>();

            // Server Refresh Intervals
            services.AddSingleton(new ServerRefresh<IDailyServerRefresh>() { Hour = 20, Interval = TimeSpan.FromDays(1) });
        }

        public static void AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoSettings:ConnectionString"]);

            IMongoDatabase database = client.GetDatabase(configuration["MongoSettings:DatabaseName"]);

            services.AddSingleton(database);
        }

        public static T AddConfigurationSingleton<T>(this IServiceCollection services, IConfiguration configuration, string section) where T: new()
        {
            T model = new T();

            configuration.Bind(section, model);

            services.AddSingleton(typeof(T), model);

            return model;
        }

        public static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            AuthenticationSettings settings = services.AddConfigurationSingleton<AuthenticationSettings>(configuration, "AuthenticationSettings");

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
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience
                };

                opt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.OnStarting(() => {

                            // Our JWT handler may throw a custom exception which will 'hopefully' force the user to
                            // invalidate and delete all local game progress.
                            if (context.Exception.GetType() == typeof(ExpiredTokenException))
                            {
                                // Client should read this header and then invalidate itself
                                context.Response.Headers.Add("Token-Expired", "true");
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
