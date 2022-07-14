using GMServer.Cache;
using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public interface IMercsService
    {
        MercsDataFile GetDataFile();
        Task<UserMerc> GetMerc(string userId, int mercId);
        Task<List<UserMerc>> GetUserMercsAsync(string userId);
        Task InsertMercAsync(UserMerc merc);
    }

    public class MercsService : IMercsService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserMerc> _mercs;

        public MercsService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _mercs = mongo.GetCollection<UserMerc>("UnlockedMercs");
        }

        public async Task InsertMercAsync(UserMerc merc)
        {
            await _mercs.InsertOneAsync(merc);
        }

        public async Task<UserMerc> GetMerc(string userId, int mercId)
        {
            return await _mercs.Find(x => x.UserID == userId && x.MercID == mercId).FirstOrDefaultAsync();
        }

        public async Task<List<UserMerc>> GetUserMercsAsync(string userId)
        {
            return await _mercs.Find(m => m.UserID == userId).ToListAsync();
        }

        /* Data File */

        public MercsDataFile GetDataFile()
        {
            return _cache.Load<MercsDataFile>(DataFiles.Mercs);
        }
    }
}
