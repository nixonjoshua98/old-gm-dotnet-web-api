using GMServer.Context;
using GMServer.Mongo.Models;
using GMServer.Mongo.Repositories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class BountyShopService
    {
        private readonly IBountyShopRepository _bountyShop;

        private readonly IMongoCollection<BountyShopPurchase> _purchases;

        public BountyShopService(IMongoDatabase mongo, IBountyShopRepository bountyShop)
        {
            _purchases = mongo.GetCollection<BountyShopPurchase>("BountyShopPurchases");
            _bountyShop = bountyShop;
        }

        public async Task InsertShopPurchaseAsync(BountyShopPurchase purchase)
        {
            await _purchases.InsertOneAsync(purchase);
        }

        public async Task<UserBountyShopState> GetUserState(string userId)
        {
            return await _bountyShop.FindOneAsync(x => x.UserID == userId);
        }

        public async Task SetUserState(UserBountyShopState state)
        {
            await _bountyShop.ReplaceOneAsync(x => x.UserID == state.UserID, state, isUpsert: true);
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
    }
}
