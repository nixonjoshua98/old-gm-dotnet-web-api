using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System.Threading.Tasks;

namespace SRC.Services
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
