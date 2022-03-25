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

        public async Task<List<BountyShopPurchase>> GetUserDailyPurchasesAsync(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return await _purchases.Find(x => x.UserID == userId && x.PurchaseTime > refresh.Previous && x.PurchaseTime < refresh.Next).ToListAsync();
        }

        public BountyShopItems GetUserBountyShop(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            int seed = $"{userId}{refresh.Previous.ToFileTimeUtc()}".GetHashCode();

            BountyShopLootTable table = new(GetDataFile(), _armoury.GetDataFile());

            return table.GetItems(5, seed);
        }

        public BountyShopDataFile GetDataFile()
        {
            return _cache.Load<BountyShopDataFile>(DataFiles.BountyShop);
        }
    }
}
