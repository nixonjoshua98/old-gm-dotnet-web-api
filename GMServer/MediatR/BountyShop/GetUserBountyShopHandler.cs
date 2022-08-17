using MediatR;
using SRC.Caching.DataFiles.Models;
using SRC.Common;
using SRC.Context;
using SRC.DataFiles.Cache;
using SRC.LootTable;
using SRC.Mongo.Models;
using SRC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.BountyShopHandler
{
    public class GetUserBountyShopRequest : IRequest<GetUserBountyShopResponse>
    {
        public string UserID;
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;
    }

    public class GetUserBountyShopResponse
    {
        public DateTime ShopCreationTime;
        public List<BountyShopPurchase> Purchases;
        public UserBountyShop ShopItems;
    }

    public class GetUserBountyShopHandler : IRequestHandler<GetUserBountyShopRequest, GetUserBountyShopResponse>
    {
        private readonly IBountiesService _bounties;
        private readonly BountyShopService _bountyshop;
        private readonly ArmouryService _armoury;
        private readonly IDataFileCache _dataFiles;
        public GetUserBountyShopHandler(BountyShopService bountyshop, ArmouryService armoury, IBountiesService bounties, IDataFileCache dataFiles)
        {
            _bounties = bounties;
            _armoury = armoury;
            _bountyshop = bountyshop;
            _dataFiles = dataFiles;
        }

        public async Task<GetUserBountyShopResponse> Handle(GetUserBountyShopRequest request, CancellationToken cancellationToken)
        {
            // Fetch the state. Purpose of the state is to prevent shop re-generations mid refresh if the user levels up etc
            UserBountyShopState shopState = await _bountyshop.GetUserState(request.UserID);

            // Check if the stored state is out dated
            bool stateIsOutdated = shopState is null || !request.DailyRefresh.IsBetween(shopState.LastUpdated);

            // Null the state if it requires updating to prevent accidently using it
            shopState = stateIsOutdated ? null : shopState;

            // Fetch the config from the state or re-calculated
            BountyShopConfig shopLevelConfig = await GetShopTableConfig(request.UserID, shopState);

            // Generate the shop items
            UserBountyShop userShop = GetItems(request.DailyRefresh, shopState, shopLevelConfig);

            // Update the state so that the next generation (if valid) will net the same results
            if (stateIsOutdated)
                await UpdateUserShopState(request.UserID, shopLevelConfig.Level, userShop.Seed);

            // Fetch the user daily purchases
            var shopPurchases = await _bountyshop.GetDailyPurchasesAsync(request.UserID, request.DailyRefresh);

            return new GetUserBountyShopResponse
            {
                ShopCreationTime = DateTime.UtcNow,
                ShopItems = userShop,
                Purchases = shopPurchases
            };
        }

        private string GetShopSeed(CurrentServerRefresh<IDailyRefresh> refresh, UserBountyShopState state)
        {
            if (state is not null)
                return state.Seed;

            return $"{refresh.Previous}";
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

        private async Task<BountyShopConfig> GetShopTableConfig(string userId, UserBountyShopState state)
        {
            var shopDataFile = _dataFiles.BountyShop;

            if (state is not null)
            {
                return shopDataFile.GetLevelConfig(state.Level);
            }

            var bountyDataFile = _dataFiles.Bounties;

            var userBounties = await _bounties.GetUserBountiesAsync(userId);

            // Calculate the total maximum hourly income
            long hourlyIncome = bountyDataFile.Bounties.Where(bounty => userBounties.UnlockedBounties.Where(x => x.BountyID == bounty.BountyID).Count() >= 1).Sum(x => x.PointsPerHour);

            return shopDataFile.GetConfig(hourlyIncome);
        }

        private UserBountyShop GetItems(CurrentServerRefresh<IDailyRefresh> refresh, UserBountyShopState state, BountyShopConfig config)
        {
            // Get the seed used to fetch the deterministic items (from state or re-calculated)
            string seed = GetShopSeed(refresh, state);

            RDSTable table = CreateShopLootTable(config);

            Random rnd = Utility.SeededRandom(seed);

            UserBountyShop shop = new() { Seed = seed };

            var results = table.GetResults(5, rnd);

            for (int i = 0; i < results.Count; i++)
            {
                var current = results[i];

                if (current is RDSValue<BSCurrencyItem> cItem)
                    shop.CurrencyItems.Add(UserBSCurrencyItem.FromShopItem(i, cItem.Value));

                else if (current is RDSValue<BSArmouryItem> aItem)
                    shop.ArmouryItems.Add(UserBSArmouryItem.FromShopItem(i, aItem.Value));
            }

            return shop;
        }


        /* Loot Table Generation Methods */

        private RDSTable CreateShopLootTable(BountyShopConfig config)
        {
            RDSTable root = new();

            root.AddEntry(CreateArmouryItemsLootTable(config));
            root.AddEntry(CreateCurrencyItemsTable(config));

            return root;
        }

        private RDSTable CreateCurrencyItemsTable(BountyShopConfig config)
        {
            RDSTable table = new()
            {
                Always = config.CurrencyItems.Always,
                Unique = config.CurrencyItems.Unique,
                Weight = config.CurrencyItems.Weight,
            };

            foreach (BSCurrencyItem item in config.CurrencyItems.Items)
            {
                RDSValue<BSCurrencyItem> itemValue = new(item)
                {
                    Always = item.Always,
                    Unique = item.Unique,
                    Weight = item.Weight,
                };

                table.AddEntry(itemValue);
            }

            return table;
        }

        private RDSTable CreateArmouryItemsLootTable(BountyShopConfig config)
        {
            List<ArmouryItem> armouryItems = _dataFiles.Armoury;

            // Create the root table for all armoury items
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
                // Find all the armoury items which belong to that grade
                List<ArmouryItem> gradeArmouryItems = armouryItems.Where(x => x.Grade == itemGrade).ToList();

                if (gradeArmouryItems.Count == 0)
                    continue;

                // Get the loot table setup for the grade item
                BSArmouryItemGradeConfig gradeItemConfig = config.ArmouryItems.ItemGrades.Find(x => x.ItemGrade == itemGrade);

                // Create the sub-table for the item grade
                RDSTable itemGradeTable = new()
                {
                    Weight = gradeItemConfig.Weight,
                    Always = gradeItemConfig.Always,
                    Unique = gradeItemConfig.Unique,
                };

                foreach (ArmouryItem item in gradeArmouryItems)
                {
                    BSArmouryItem value = new()
                    {
                        ID = item.ID,
                        PurchaseCost = gradeItemConfig.PurchaseCost
                    };

                    // Create the table value for the armoury item
                    RDSValue<BSArmouryItem> tableValue = new(value);

                    itemGradeTable.AddEntry(tableValue);
                }

                table.AddEntry(itemGradeTable);
            }

            return table;
        }
    }
}
