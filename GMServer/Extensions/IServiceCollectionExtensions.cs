using GMServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

namespace GMServer
{
    public static class IServiceCollectionExtensions
    {
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

        public static void AddJWTAuthentication(this IServiceCollection services, AuthenticationSettings settings)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Key)),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                };
            });
        }
    }
}
