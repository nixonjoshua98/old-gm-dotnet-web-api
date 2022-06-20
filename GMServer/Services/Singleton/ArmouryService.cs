using GMServer.Cache;
using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class ArmouryService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserArmouryItem> _armoury;

        public ArmouryService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _armoury = mongo.GetCollection<UserArmouryItem>("ArmouryItems");
        }

        public async Task<UserArmouryItem> GetArmouryItemAsync(string userId, int itemId)
        {
            return await _armoury.Find(x => x.UserID == userId && x.ItemID == itemId).FirstOrDefaultAsync();
        }

        public async Task<List<UserArmouryItem>> GetUserArmouryItemsAsync(string userId)
        {
            return await _armoury.Find(x => x.UserID == userId).ToListAsync();
        }

        public async Task<UserArmouryItem> IncrementItemAsync(string userId, int itemId, UserArmouryItem incr, bool upsert = false)
        {
            var filter = Builders<UserArmouryItem>.Filter.Eq(x => x.UserID, userId) & Builders<UserArmouryItem>.Filter.Eq(x => x.ItemID, itemId);
            var update = Builders<UserArmouryItem>.Update
                .Inc(x => x.Level, incr.Level)
                .Inc(x => x.Owned, incr.Owned);

            return await _armoury.FindOneAndUpdateAsync(filter, update, new() { IsUpsert = upsert, ReturnDocument = ReturnDocument.After });
        }

        public List<ArmouryItem> GetDataFile()
        {
            return _cache.Load<List<ArmouryItem>>(DataFiles.Armoury);
        }
    }
}
