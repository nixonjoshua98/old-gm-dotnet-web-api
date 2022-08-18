using SRC.Caching.DataFiles.Models;
using SRC.Common;
using SRC.Common.Enums;
using SRC.DataFiles.Cache;
using SRC.LootTable;
using SRC.Mongo.Models;
using SRC.Services.BountyShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRC.Services.BountyShop
{
    public interface IBountyShopFactory
    {
        Task<GeneratedBountyShop> GenerateBountyShopAsync(string userId, int gameDayNumber);
        Task<GeneratedBountyShop> GenerateBountyShopAsync(string userId);
    }

    public class BountyShopFactory : IBountyShopFactory
    {
        private readonly IBountiesService _bounties;
        private readonly BountyShopService _bountyshop;
        private readonly ArmouryService _armoury;
        private readonly IDataFileCache _dataFiles;

        public BountyShopFactory(BountyShopService bountyshop,
                                 ArmouryService armoury,
                                 IBountiesService bounties,
                                 IDataFileCache dataFiles)
        {
            _bounties = bounties;
            _armoury = armoury;
            _bountyshop = bountyshop;
            _dataFiles = dataFiles;
        }

        public async Task<GeneratedBountyShop> GenerateBountyShopAsync(string userId)
        {
            return await GenerateBountyShopAsync(userId, Utility.GetGameDayNumber());
        }

        public async Task<GeneratedBountyShop> GenerateBountyShopAsync(string userId, int gameDayNumber)
        {
            BountyShopConfig shopConfig = _dataFiles.BountyShop.GetLevelConfig(1);

            RDSTable lootTable = CreateLootTable(shopConfig);

            UserBountyShop shopItems = CreateShopFromLootTable(lootTable, $"{gameDayNumber}");

            return new GeneratedBountyShop()
            {
                GameDayNumber = gameDayNumber,
                ShopItems = shopItems,
                Purchases = new()
            };
        }
        
        private UserBountyShop CreateShopFromLootTable(RDSTable lootTable, string seed)
        {
            Random rnd = Utility.SeededRandom(seed);

            UserBountyShop shop = new();

            var results = lootTable.GetResults(10, rnd);

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

        private RDSTable CreateLootTable(BountyShopConfig config)
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
            IEnumerable<Rarity> itemGrades = config.ArmouryItems.ItemGrades.Select(x => x.ItemGrade).Distinct();

            foreach (Rarity itemGrade in itemGrades)
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
                        ItemID = item.ID,
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
