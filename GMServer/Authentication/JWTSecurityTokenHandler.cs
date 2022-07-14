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
        private readonly IUserService _users;

        public JWTSecurityTokenHandler(IServiceProvider serviceProvider)
        {
            _auth = serviceProvider.GetRequiredService<AuthenticationService>();
            _users = serviceProvider.GetRequiredService<IUserService>();
        }

        public override bool CanReadToken(string token) => true;

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            User user = _users.GetUserByAccessToken(token);

            if (user is null)
            {
                throw new InvalidTokenException();
            }

            string jwtToken = _auth.CreateToken(user);

            return base.ValidateToken(jwtToken, validationParameters, out validatedToken);
        }
    }
}
