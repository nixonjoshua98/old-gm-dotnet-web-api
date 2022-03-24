using GMServer.Exceptions;
using GMServer.Models;
using GMServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMServer.Authentication
{
    public class JWTSecurityTokenHandler : JwtSecurityTokenHandler
    {
        private readonly AuthenticationService _auth;

        public JWTSecurityTokenHandler(IServiceProvider serviceProvider)
        {
            _auth = serviceProvider.GetRequiredService<AuthenticationService>();
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var claimsPrincipal = base.ValidateToken(token, validationParameters, out validatedToken);

            AuthenticatedSession session = _auth.GetSession(token);

            if (session is null || !session.IsValid)
            {
                throw new ExpiredTokenException();
            }

            return claimsPrincipal;
        }
    }
}
