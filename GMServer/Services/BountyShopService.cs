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

        public BountyShopService(IMongoDatabase mongo, IDataFileCache cache)
        {
            _cache = cache;
            _purchases = mongo.GetCollection<BountyShopPurchase>("BountyShopPurchases");
        }

        public async Task InsertShopPurchaseAsync(BountyShopPurchase bountyShopPurchase)
        {
            //await _purchases.InsertOneAsync(bountyShopPurchase);
        }

        public async Task<List<BountyShopPurchase>> GetDailyPurchasesAsync(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return await _purchases.Find(x =>
                x.UserID == userId &&
                x.PurchaseTime > refresh.Previous &&
                x.PurchaseTime < refresh.Next).ToListAsync();
        }

        public async Task<List<BountyShopPurchase>> GetDailyItemPurchasesAsync(string userId, string itemId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return await _purchases.Find(x =>
                x.UserID == userId &&
                x.ItemID == itemId &&
                x.PurchaseTime > refresh.Previous &&
                x.PurchaseTime < refresh.Next).ToListAsync();
        }

        public BountyShopDataFile GetDataFile()
        {
            return _cache.Load<BountyShopDataFile>(DataFiles.BountyShop);
        }
    }
}
