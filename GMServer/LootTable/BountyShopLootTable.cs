using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using System;
using System.Collections.Generic;

namespace GMServer.LootTable
{
    public class BountyShopLootTable
    {
        private readonly RDSTable RootTable;
        private readonly BountyShopDataFile bountyshopDataFile;
        private readonly List<ArmouryItem> armouryDataFile;

        public BountyShopLootTable(BountyShopDataFile datafile, List<ArmouryItem> armouryIitems)
        {
            RootTable = new();

            armouryDataFile = armouryIitems;
            bountyshopDataFile = datafile;

            AddCurrencyItemsTable();
            AddArmouryItemsTable();
        }

        public BountyShopItems GetItems(int count, string seed)
        {
            Random rnd = Utility.SeededRandom(seed);

            BountyShopItems shop = new();

            var results = RootTable.GetResults(count, rnd);

            for (int i = 0; i < results.Count; i++)
            {
                var current = results[i];

                if (current is RDSValue<BountyShopCurrencyItem> cItem)
                {
                    shop.CurrencyItems.Add(new()
                    {
                        ID = $"CI{i}{rnd.NextInt64()}",
                        Quantity = cItem.Value.Quantity,
                        CurrencyType = cItem.Value.CurrencyType,
                        PurchaseCost = cItem.Value.PurchaseCost,
                    });
                }
                else if (current is RDSValue<BountyShopArmouryItem> aItem)
                {
                    shop.ArmouryItems.Add(new()
                    {
                        ID = $"AI{i}{rnd.NextInt64()}",
                        ItemID = aItem.Value.ID,
                        PurchaseCost = aItem.Value.PurchaseCost
                    });
                }
            }

            return shop;
        }

        private void AddArmouryItemsTable()
        {
            RDSTable table = new()
            {
                Always = bountyshopDataFile.ArmouryItems.Always,
                Unique = bountyshopDataFile.ArmouryItems.Unique,
                Weight = bountyshopDataFile.ArmouryItems.Weight,
            };

            foreach (ArmouryItem item in armouryDataFile)
            {
                BountyShopArmouryItem bsItem = new()
                {
                    ID = item.ID,
                    PurchaseCost = bountyshopDataFile.ArmouryItems.PurchaseCost
                };

                RDSValue<BountyShopArmouryItem> rdsItem = new(bsItem)
                {

                };

                table.AddEntry(rdsItem);
            }

            RootTable.AddEntry(table);
        }

        private void AddCurrencyItemsTable()
        {
            RDSTable currencyItemsTable = new()
            {
                Always = bountyshopDataFile.CurrencyItems.Always,
                Unique = bountyshopDataFile.CurrencyItems.Unique,
                Weight = bountyshopDataFile.CurrencyItems.Weight,
            };

            foreach (BountyShopCurrencyItem item in bountyshopDataFile.CurrencyItems.Items)
            {
                RDSValue<BountyShopCurrencyItem> itemValue = new(item)
                {
                    Always = item.Always,
                    Unique = item.Unique,
                    Weight = item.Weight,
                };

                currencyItemsTable.AddEntry(itemValue);
            }

            RootTable.AddEntry(currencyItemsTable);
        }
    }
}
