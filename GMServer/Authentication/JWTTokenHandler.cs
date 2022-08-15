using GMServer.Common.Exceptions;
using GMServer.Models.Settings;
using GMServer.Mongo.Models;
using GMServer.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GMServer.Authentication
{
    public class JWTTokenHandler : JwtSecurityTokenHandler
    {
        private readonly IUserService _users;
        private readonly JWTBearerSettings _settings;

        public JWTTokenHandler(IUserService users, JWTBearerSettings settings)
        {
            _users = users;
            _settings = settings;
        }

        public override bool CanReadToken(string token) => true;

        private string CreateToken(User user)
        {
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.UserID, user.ID)
                }),
                SigningCredentials = _settings.SigningCredentials
            };

            return WriteToken(CreateToken(descriptor));
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            User user = _users.GetUserByAccessToken(token);

            if (user is null)
            {
                throw new InvalidTokenException();
            }

            string jwtToken = CreateToken(user);

            return base.ValidateToken(jwtToken, validationParameters, out validatedToken);
        }
    }
}
