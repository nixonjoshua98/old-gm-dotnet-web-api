using GMServer.Authentication;
using GMServer.UserModels;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class AuthenticationService
    {
        private readonly AuthenticationSettings _settings;
        private readonly IMongoCollection<AuthenticatedSession> _sessions;

        public AuthenticationService(AuthenticationSettings settings, IMongoDatabase mongo)
        {
            _settings = settings;
            _sessions = mongo.GetCollection<AuthenticatedSession>("AuthenticationSessions");
        }

        public AuthenticatedSession GetSession(string token)
        {
            return _sessions.Find(doc => doc.Token == token).FirstOrDefault();
        }

        public void InvalidateSession(string token)
        {
            var update = Builders<AuthenticatedSession>.Update
                .Set(s => s.IsValid, false);

            _sessions.UpdateMany(x => x.IsValid && x.Token == token, update);
        }

        public async Task InvalidateUserSessionsAsync(User user)
        {
            var update = Builders<AuthenticatedSession>.Update
                .Set(s => s.IsValid, false);

            await _sessions.UpdateManyAsync(x => x.IsValid && x.UserID == user.ID, update);
        }

        public async Task InsertSessionAsync(AuthenticatedSession session)
        {
            await _sessions.InsertOneAsync(session);
        }

        public string CreateToken(User user)
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes(_settings.Key);

            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimNames.UserID, user.ID),
                }),
                Audience = _settings.Audience,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }
    }
}
