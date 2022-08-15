using GMServer.Mongo.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class ArmouryService
    {
        private readonly IMongoCollection<UserArmouryItem> _armoury;

        public ArmouryService(IMongoDatabase mongo)
        {
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
    }
}
