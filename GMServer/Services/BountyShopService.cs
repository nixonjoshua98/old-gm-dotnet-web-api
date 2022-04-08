using GMServer.Cache;
using GMServer.Common;
using GMServer.Context;
using GMServer.LootTable;
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
        private readonly ArmouryService _armoury;
        private readonly IMongoCollection<BountyShopPurchase> _purchases;

        public BountyShopService(IMongoDatabase mongo, IDataFileCache cache, ArmouryService armoury)
        {
            _cache = cache;
            _armoury = armoury;
            _purchases = mongo.GetCollection<BountyShopPurchase>("BountyShopPurchases");
        }

        public async Task InsertShopPurchaseAsync(BountyShopPurchase bountyShopPurchase)
        {
            await _purchases.InsertOneAsync(bountyShopPurchase);
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

        public BountyShopItems GetUserBountyShop(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            BountyShopLootTable table = new(GetDataFile(), _armoury.GetDataFile());

            // Seed is on a per-user basis
            return table.GetItems(16, $"{userId}-{refresh.Previous}");
        }

        public BountyShopDataFile GetDataFile()
        {
            return _cache.Load<BountyShopDataFile>(DataFiles.BountyShop);
        }
    }
}
