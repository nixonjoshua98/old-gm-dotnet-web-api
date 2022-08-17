using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SRC.Services
{
    public interface IMercsService
    {
        Task<UserMerc> GetMerc(string userId, int mercId);
        Task<List<UserMerc>> GetUserMercsAsync(string userId);
        Task InsertMercAsync(UserMerc merc);
    }

    public class MercsService : IMercsService
    {
        private readonly IMercRepository _mercs;

        public MercsService(IMercRepository mercs)
        {
            _mercs = mercs;
        }

        public async Task InsertMercAsync(UserMerc merc)
        {
            await _mercs.InsertOneAsync(merc);
        }

        public async Task<UserMerc> GetMerc(string userId, int mercId)
        {
            return await _mercs.FindOneAsync(x => x.UserID == userId && x.MercID == mercId);
        }

        public async Task<List<UserMerc>> GetUserMercsAsync(string userId)
        {
            return await _mercs.FindAsync(m => m.UserID == userId);
        }
    }
}
