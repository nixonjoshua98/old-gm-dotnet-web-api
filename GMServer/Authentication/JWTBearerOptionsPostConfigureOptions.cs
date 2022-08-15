using GMServer.Common.Exceptions;
using GMServer.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace GMServer.Authentication
{
    public class JWTBearerOptionsPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly JWTTokenHandler _tokenValidator;
        private readonly JWTBearerSettings _settings;

        public JWTBearerOptionsPostConfigureOptions(JWTTokenHandler tokenValidator, JWTBearerSettings settings)
        {
            _tokenValidator = tokenValidator;
            _settings = settings;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            options.SecurityTokenValidators.Clear();
            options.SecurityTokenValidators.Add(_tokenValidator);

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new()
            {
                IssuerSigningKey = _settings.SymmetricSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateAudience = false,
            };

            options.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    context.Response.OnStarting(() =>
                    {
                        // Our JWT handler may throw a custom exception which will 'hopefully' force the user to
                        // invalidate and delete all local game progress.
                        // // Client should read this header and then invalidate itself
                        if (context.Exception is InvalidTokenException exc)
                        {
                            context.Response.Headers.Add("Invalid-Token", "true");
                        }

                        return Task.CompletedTask;
                    });

                    return Task.CompletedTask;
                }
            };
        }
    }
}
