using GMServer.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class AuthenticationService
    {
        private readonly AuthenticationSettings _settings;
        private readonly IMongoCollection<AuthenticationSession> _sessions;

        public AuthenticationService(AuthenticationSettings settings, IMongoDatabase mongo)
        {
            _settings = settings;
            _sessions = mongo.GetCollection<AuthenticationSession>("AuthenticationSessions");
        }

        public AuthenticationSession GetSession(string token)
        {
            return _sessions.Find(doc => doc.Token == token).FirstOrDefault();
        }

        public void InvalidateToken(string token)
        {
            var update = Builders<AuthenticationSession>.Update
                .Set(s => s.IsValid, false);

            _sessions.FindOneAndUpdate(x => x.Token == token, update);
        }

        public async Task InvalidateUserSessionsAsync(User user)
        {
            var update = Builders<AuthenticationSession>.Update
                .Set(s => s.IsValid, false);

            await _sessions.UpdateManyAsync(x => x.UserID == user.ID, update);
        }

        public async Task InsertSessionAsync(AuthenticationSession session)
        {
            await _sessions.InsertOneAsync(session);
        }

        public string CreateToken()
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes(_settings.Key);

            SecurityTokenDescriptor descriptor = new()
            {
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            return handler.WriteToken(token);
        }
    }
}
