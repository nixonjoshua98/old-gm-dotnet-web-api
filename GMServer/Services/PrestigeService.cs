using GMServer.Mongo.Models;
using GMServer.Mongo.Repositories;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class PrestigeService
    {
        private readonly IPrestigeRepository _prestiges;

        public PrestigeService(IPrestigeRepository prestiges)
        {
            _prestiges = prestiges;
        }

        public async Task InsertPrestigeLogAsync(UserPrestige prestige)
        {
            await _prestiges.InsertOneAsync(prestige);
        }
    }
}
