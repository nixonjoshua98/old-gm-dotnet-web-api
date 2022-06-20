using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class PrestigeService
    {
        private readonly IMongoCollection<UserPrestige> _prestigeLogs;

        public PrestigeService(IMongoDatabase mongo)
        {
            _prestigeLogs = mongo.GetCollection<UserPrestige>("UserPrestiges");
        }

        public async Task InsertPrestigeLogAsync(UserPrestige prestige)
        {
            await _prestigeLogs.InsertOneAsync(prestige);
        }
    }
}
