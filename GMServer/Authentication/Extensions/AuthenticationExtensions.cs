using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SRC.Models.Settings;

namespace SRC.Authentication.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWTBearerSettings>(configuration, "AuthenticationSettings");

            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JWTBearerOptionsPostConfigureOptions>();

            services.AddSingleton<JWTTokenHandler>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();
        }
    }
}
