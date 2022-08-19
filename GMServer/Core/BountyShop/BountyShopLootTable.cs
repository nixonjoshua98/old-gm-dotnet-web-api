using SRC.Caching.DataFiles.Models;
using SRC.Common;
using SRC.Common.Enums;
using SRC.Core.BountyShop.Models;
using SRC.DataFiles.Cache;
using SRC.LootTable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRC.Core.BountyShop
{
    public class BountyShopLootTable
    {
        private readonly IDataFileCache _datafiles;
        private readonly BountyShopConfig _shopConfig;

        private readonly RDSTable Table = new();

        public BountyShopLootTable(IDataFileCache datafiles, BountyShopConfig shopConfig)
        {
            _datafiles = datafiles;
            _shopConfig = shopConfig;

            Create();
        }

        public void Create()
        {
            Table.AddEntry(CreateCurrencyItemsTable());
            Table.AddEntry(CreateArmouryItemsLootTable());
        }

        public BountyShopItems GetItems(string seed, int numItems)
        {
            Random rnd = Utility.SeededRandom(seed);

            BountyShopItems shop = new();

            var results = Table.GetResults(numItems, rnd);

            for (int i = 0; i < results.Count; i++)
            {
                var current = results[i];

                if (current is RDSValue<BSCurrencyItem> cItem)
                    shop.CurrencyItems.Add(new($"CI{seed}{i}", cItem.Value.CurrencyType, cItem.Value.Quantity, cItem.Value.PurchaseCost));

                else if (current is RDSValue<BSArmouryItem> aItem)
                    shop.ArmouryItems.Add(new($"AI{seed}{i}", aItem.Value.ItemID, aItem.Value.PurchaseCost));
            }

            return shop;
        }

        private RDSTable CreateCurrencyItemsTable()
        {
            var config = _shopConfig.CurrencyItems;

            RDSTable table = new(config.Weight, config.Unique, config.Always);

            foreach (BSCurrencyItem item in config.Items)
            {
                table.AddEntry(new RDSValue<BSCurrencyItem>(value: item, weight: item.Weight, unique: item.Unique, always: item.Always));
            }

            return table;
        }

        private RDSTable CreateArmouryItemsLootTable()
        {
            BSArmouryItems shopItems = _shopConfig.ArmouryItems;

            List<ArmouryItem> armouryItems = _datafiles.Armoury;

            // Create the root table for all armoury items
            RDSTable table = new(shopItems.Weight, shopItems.Unique, shopItems.Always);

            // Find all unique item grades which are available
            IEnumerable<Rarity> itemGrades = shopItems.ItemGrades.Select(x => x.ItemGrade).Distinct();

            foreach (Rarity itemGrade in itemGrades)
            {
                // Find all the armoury items which belong to that grade
                List<ArmouryItem> gradeArmouryItems = armouryItems.Where(x => x.Grade == itemGrade).ToList();

                if (gradeArmouryItems.Count == 0)
                    continue;

                // Get the loot table setup for the grade item
                BSArmouryItemGradeConfig gradeItemConfig = shopItems.ItemGrades.FirstOrDefault(x => x.ItemGrade == itemGrade);

                if (gradeItemConfig is null)
                    continue;

                // Create the sub-table for the item grade
                RDSTable itemRarityTable = new(gradeItemConfig.Weight, gradeItemConfig.Unique, gradeItemConfig.Always);

                foreach (ArmouryItem item in gradeArmouryItems)
                {
                    BSArmouryItem value = new(item.ID, gradeItemConfig.PurchaseCost);

                    itemRarityTable.AddEntry(new RDSValue<BSArmouryItem>(value));
                }

                table.AddEntry(itemRarityTable);
            }

            return table;
        }
    }
}
