using GMServer.Common;
using GMServer.Context;
using GMServer.LootTable;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyShopHandler
{
    public class GetUserBountyShopRequest : IRequest<GetUserBountyShopResponse>
    {
        public string UserID;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public class GetUserBountyShopResponse
    {
        public DateTime ShopCreationTime;
        public List<BountyShopPurchase> Purchases;
        public BountyShopItems ShopItems;
    }

    public class GetUserBountyShopHandler : IRequestHandler<GetUserBountyShopRequest, GetUserBountyShopResponse>
    {
        private readonly BountiesService _bounties;
        private readonly BountyShopService _bountyshop;
        private readonly ArmouryService _armoury;

        public GetUserBountyShopHandler(BountyShopService bountyshop, ArmouryService armoury, BountiesService bounties)
        {
            _bounties = bounties;
            _armoury = armoury;
            _bountyshop = bountyshop;
        }

        public async Task<GetUserBountyShopResponse> Handle(GetUserBountyShopRequest request, CancellationToken cancellationToken)
        {
            // Fetch the state. Purpose of the state is to prevent shop re-generations mid refresh if the user levels up etc
            UserBountyShopState shopState = await _bountyshop.GetUserState(request.UserID);

            // Check if the stored state is out dated
            bool requiresStateUpdate = shopState is null || !request.DailyRefresh.IsBetween(shopState.LastUpdated);

            // Null the state if it requires updating to prevent accidently using it
            shopState = requiresStateUpdate ? null : shopState;

            // Fetch the config from the state or re-calculated
            BountyShopLootTableConfig shopLevelConfig = await GetShopTableConfig(request.UserID, shopState);

            // Get the seed used to fetch the deterministic items (from state or re-calculated)
            string seed = GetShopSeed(request.UserID, request.DailyRefresh, shopState);

            // Generate the shop items
            BountyShopItems items = GetItems(shopLevelConfig, 5, seed);

            // Update the state so that the next generation (if valid) will net the same results
            if (requiresStateUpdate)
                await UpdateUserShopState(request.UserID, shopLevelConfig.Level, seed);

            // Fetch the user daily purchases
            var shopPurchases = await _bountyshop.GetDailyPurchasesAsync(request.UserID, request.DailyRefresh);

            return new GetUserBountyShopResponse
            {
                ShopCreationTime = DateTime.UtcNow,
                ShopItems = items,
                Purchases = shopPurchases
            };
        }

        private string GetShopSeed(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh, UserBountyShopState state)
        {
            if (state is not null)
                return state.Seed;

            return $"{userId} | {refresh.Previous}";
        }

        private async Task UpdateUserShopState(string userId, int level, string seed)
        {
            UserBountyShopState state = new()
            {
                UserID = userId,
                Seed = seed,
                LastUpdated = DateTime.UtcNow,
                Level = level
            };

            await _bountyshop.SetUserState(state);

        }

        private async Task<BountyShopLootTableConfig> GetShopTableConfig(string userId, UserBountyShopState state)
        {
            var shopDataFile = _bountyshop.GetDataFile();

            if (state is not null)
            {
                return shopDataFile.GetLevelConfig(state.Level);
            }

            var bountyDataFile = _bounties.GetDataFile();

            var userBounties = await _bounties.GetUserBountiesAsync(userId);

            // Calculate the total maximum hourly income
            long hourlyIncome = bountyDataFile.Bounties.Where(bounty => userBounties.UnlockedBounties.Where(x => x.BountyID == bounty.BountyID).Count() >= 1).Sum(x => x.HourlyIncome);

            return shopDataFile.GetConfig(hourlyIncome);
        }

        private BountyShopItems GetItems(BountyShopLootTableConfig config, int count, string seed)
        {
            RDSTable table = CreateShopLootTable(config);

            Random rnd = Utility.SeededRandom(seed);

            BountyShopItems shop = new();

            var results = table.GetResults(count, rnd);

            for (int i = 0; i < results.Count; i++)
            {
                var current = results[i];

                if (current is RDSValue<BountyShopCurrencyItem> cItem)
                {
                    shop.CurrencyItems.Add(new()
                    {
                        ID = $"CI-{i}",
                        Quantity = cItem.Value.Quantity,
                        CurrencyType = cItem.Value.CurrencyType,
                        PurchaseCost = cItem.Value.PurchaseCost,
                    });
                }
                else if (current is RDSValue<BountyShopArmouryItem> aItem)
                {
                    shop.ArmouryItems.Add(new()
                    {
                        ID = $"AI-{i}",
                        ItemID = aItem.Value.ID,
                        PurchaseCost = aItem.Value.PurchaseCost
                    });
                }
            }

            return shop;
        }

        private RDSTable CreateShopLootTable(BountyShopLootTableConfig config)
        {
            RDSTable root = new();

            root.AddEntry(CreateArmouryItemsLootTable(config));
            root.AddEntry(CreateCurrencyItemsTable(config));

            return root;
        }

        private RDSTable CreateCurrencyItemsTable(BountyShopLootTableConfig config)
        {
            RDSTable table = new()
            {
                Always = config.CurrencyItems.Always,
                Unique = config.CurrencyItems.Unique,
                Weight = config.CurrencyItems.Weight,
            };

            foreach (BountyShopCurrencyItem item in config.CurrencyItems.Items)
            {
                RDSValue<BountyShopCurrencyItem> itemValue = new(item)
                {
                    Always = item.Always,
                    Unique = item.Unique,
                    Weight = item.Weight,
                };

                table.AddEntry(itemValue);
            }

            return table;
        }

        private RDSTable CreateArmouryItemsLootTable(BountyShopLootTableConfig config)
        {
            List<ArmouryItem> armouryItems = _armoury.GetDataFile();

            RDSTable table = new()
            {
                Always = config.ArmouryItems.Always,
                Unique = config.ArmouryItems.Unique,
                Weight = config.ArmouryItems.Weight,
            };

            // Find all unique item grades which are available
            IEnumerable<ItemGrade> itemGrades = config.ArmouryItems.ItemGrades.Select(x => x.ItemGrade).Distinct();

            foreach (ItemGrade itemGrade in itemGrades)
            {
                // Get the loot table setup for the grade item
                BountyShopArmouryItemGradeLootItem gradeItemConfig = config.ArmouryItems.ItemGrades.Find(x => x.ItemGrade == itemGrade);

                // Find all the armoury items which belong to that grade
                List<ArmouryItem> gradeArmouryItems = armouryItems.Where(x => x.Grade == itemGrade).ToList();

                if (gradeArmouryItems.Count == 0)
                    continue;

                // Create the sub-table for the item grade
                RDSTable itemGradeTable = new()
                {
                    Weight = gradeItemConfig.Weight,
                    Always = gradeItemConfig.Always,
                    Unique = gradeItemConfig.Unique,
                };

                foreach (ArmouryItem item in gradeArmouryItems)
                {
                    BountyShopArmouryItem value = new()
                    {
                        ID = item.ID,
                        PurchaseCost = gradeItemConfig.PurchaseCost
                    };

                    // Create the table value for the armoury item
                    RDSValue<BountyShopArmouryItem> tableValue = new(value);

                    itemGradeTable.AddEntry(tableValue);
                }

                table.AddEntry(itemGradeTable);
            }

            return table;
        }
    }
}
