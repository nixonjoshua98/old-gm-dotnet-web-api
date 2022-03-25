using GMServer.Context;
using GMServer.LootTable;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;

namespace GMServer.Services
{
    public class BountyShopService
    {
        private readonly IDataFileCache _cache;
        private readonly ArmouryService _armoury;

        public BountyShopService(IDataFileCache cache, ArmouryService armoury)
        {
            _cache = cache;
            _armoury = armoury;
        }

        public UserBountyShop GetUserBountyShop(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            int seed = $"{userId}{refresh.Previous.ToFileTimeUtc()}".GetHashCode();

            BountyShopLootTable table = new(GetDataFile(), _armoury.GetDataFile(), 5);

            return table.GetItems(seed);
        }

        public BountyShopDataFile GetDataFile()
        {
            return _cache.Load<BountyShopDataFile>(DataFiles.BountyShop);
        }
    }
}
