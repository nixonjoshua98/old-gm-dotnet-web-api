using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System.Threading.Tasks;

namespace SRC.Services.BountyShop
{
    public class BountyShopService
    {
        private readonly IBountyShopRepository _bountyShop;

        public BountyShopService(IBountyShopRepository shop)
        {
            _bountyShop = shop;
        }

        public async Task<BountyShopModel> GetUserShopAsync(string userId, int gameDayNumber)
        {
            return await _bountyShop.FindOneAsync(doc => doc.UserID == userId && doc.GameDayNumber == 0);
        }

        public async Task AddShopPurchaseAsync(string userId, int gameDayNumber, BountyShopPurchaseModel purchase)
        {
            var filter = _bountyShop.Filter.Where(doc => doc.UserID == userId && doc.GameDayNumber == gameDayNumber);
            var update = _bountyShop.Update.Push(doc => doc.Purchases, purchase);

            //await _bountyShop.UpdateOneAsync(filter, update, upsert: true);
        }
    }
}
