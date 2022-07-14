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
            services.AddScoped<AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDataFileCache, DataFileCache>();
            services.AddScoped<ArtefactsService>();
            services.AddScoped<MercsService>();
            services.AddScoped<ArmouryService>();
            services.AddScoped<AccountStatsService>();
            services.AddScoped<IBountiesService, BountiesService>();
            services.AddScoped<QuestsService>();
            services.AddScoped<PrestigeService>();
            services.AddScoped<CurrenciesService>();
            services.AddScoped<BountyShopService>();

            // Scoped
            services.AddScoped<RequestContext>();

            // Server Refresh Intervals
            services.AddSingleton(new ServerRefresh<IDailyRefresh>() { Hour = 20, Interval = TimeSpan.FromDays(1) });
        }

        public static void AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            IMongoClient client     = new MongoClient(configuration["MongoSettings:ConnectionString"]);
            IMongoDatabase database = client.GetDatabase("GMDatabase");

            services.AddSingleton(database);
            services.AddSingleton(client);

            //services.AddScoped<IMongoContext, MongoContext>();
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
                    ValidateAudience = false,
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
