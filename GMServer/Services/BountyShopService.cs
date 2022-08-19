using MongoDB.Driver;
using SRC.Caching.DataFiles.Models;
using SRC.Common;
using SRC.Core.BountyShop;
using SRC.DataFiles.Cache;
using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System.Threading.Tasks;

namespace SRC.Services
{
    public interface IBountyShopService
    {
        Task AddShopPurchaseAsync(IClientSessionHandle session, string userId, int gameDayNumber, BountyShopPurchaseModel purchase);
        Task<GeneratedBountyShop> GetUserShopAsync(string userId, int? gameDayNumber = null);
    }

    public class BountyShopService : IBountyShopService
    {
        private readonly IBountyShopRepository _bountyShop;
        private readonly IDataFileCache _datafiles;

        public BountyShopService(IBountyShopRepository shop, IDataFileCache datafiles)
        {
            _bountyShop = shop;
            _datafiles = datafiles;
        }

        public async Task<GeneratedBountyShop> GetUserShopAsync(string userId, int? nullableGameDay = null)
        {
            int gameDayNumber = nullableGameDay ?? Utility.GetGameDayNumber();

            BountyShopModel mongoShop = await GetUserAsync(userId, gameDayNumber);
            BountyShopConfig shopConfig = _datafiles.BountyShop.GetLevelConfig(1);
            BountyShopLootTable lootTable = new(_datafiles, shopConfig);

            return new GeneratedBountyShop()
            {
                GameDayNumber = gameDayNumber,
                Purchases = (mongoShop ?? new()).Purchases,
                ShopItems = lootTable.GetItems($"{gameDayNumber}", 15)
            };
        }

        private async Task<BountyShopModel> GetUserAsync(string userId, int gameDayNumber)
        {
            return await _bountyShop.FindOneAsync(doc => doc.UserID == userId && doc.GameDayNumber == gameDayNumber);
        }

        public async Task AddShopPurchaseAsync(IClientSessionHandle session, string userId, int gameDayNumber, BountyShopPurchaseModel purchase)
        {
            var filter = _bountyShop.Filter.Where(doc => doc.UserID == userId && doc.GameDayNumber == gameDayNumber);
            var update = _bountyShop.Update.Push(doc => doc.Purchases, purchase);

            await _bountyShop.UpdateOneAsync(session, filter, update, upsert: true);
        }
    }
}
