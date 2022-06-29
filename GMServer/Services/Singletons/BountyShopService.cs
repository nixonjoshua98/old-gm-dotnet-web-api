using GMServer.Cache;
using GMServer.Common;
using GMServer.Context;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class BountyShopService
    {
        private readonly IDataFileCache _cache;

        private readonly IMongoCollection<BountyShopPurchase> _purchases;
        private readonly IMongoCollection<UserBountyShopState> _states;

        public BountyShopService(IMongoDatabase mongo, IDataFileCache cache)
        {
            _cache = cache;

            _purchases = mongo.GetCollection<BountyShopPurchase>("BountyShopPurchases");
            _states = mongo.GetCollection<UserBountyShopState>("BountyShopStates");
        }

        public async Task InsertShopPurchaseAsync(BountyShopPurchase purchase)
        {
            await _purchases.InsertOneAsync(purchase);
        }

        public async Task<UserBountyShopState> GetUserState(string userId)
        {
            return await _states.Find(x => x.UserID == userId).FirstOrDefaultAsync();
        }

        public async Task SetUserState(UserBountyShopState state)
        {
            await _states.ReplaceOneAsync(x => x.UserID == state.UserID, state, new ReplaceOptions() { IsUpsert = true });
        }

        public async Task<List<BountyShopPurchase>> GetDailyPurchasesAsync(string userId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            return await _purchases.Find(x =>
                x.UserID == userId &&
                x.PurchaseTime >= refresh.Previous &&
                x.PurchaseTime < refresh.Next).ToListAsync();
        }

        public async Task<BountyShopPurchase> GetPurchasedItemAsync(string userId, string itemId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            return await _purchases.Find(x =>
                x.UserID == userId &&
                x.ItemID == itemId &&
                x.PurchaseTime >= refresh.Previous &&
                x.PurchaseTime < refresh.Next).FirstOrDefaultAsync();
        }

        public BountyShopDataFile GetDataFile()
        {
            return _cache.Load<BountyShopDataFile>(DataFiles.BountyShop);
        }
    }
}
