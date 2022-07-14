using GMServer.Authentication;
using GMServer.Models;
using GMServer.Models.Settings;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace GMServer.Services
{
    public class AuthenticationService
    {
        private JWTBearerSettings _settings;

        public AuthenticationService(JWTBearerSettings settings)
        {
            _settings = settings;
        }

        public string GenerateAccessToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 64)
                             .Select(s => s[Random.Shared.Next(s.Length)])
                             .ToArray());
        }

        public string CreateToken(User user)
        {
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimNames.UserID, user.ID)
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_settings.KeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }
    }
}
