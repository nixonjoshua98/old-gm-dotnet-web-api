using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.UserModels.DataFileModels;
using System;
using System.Collections.Generic;

namespace GMServer.LootTable
{
    public class BountyShopLootTable
    {
        RDSTable RootTable;
        BountyShopDataFile bountyshopDataFile;
        List<ArmouryItem> armouryDataFile;

        public BountyShopLootTable(BountyShopDataFile datafile, List<ArmouryItem> armouryIitems)
        {
            RootTable = new();

            armouryDataFile = armouryIitems;
            bountyshopDataFile = datafile;

            AddCurrencyItemsTable();
            AddArmouryItemsTable();
        }

        public BountyShopItems GetItems(int count, int seed)
        {
            Random rnd = new Random(seed);

            BountyShopItems shop = new();

            foreach (var x in RootTable.GetResults(count, rnd))
            {
                Console.WriteLine(x);
                if (x is RDSValue<BountyShopCurrencyItem> cItem)
                {
                    shop.CurrencyItems.Add(new()
                    {
                        Quantity = cItem.Value.Quantity,
                        CurrencyType = cItem.Value.CurrencyType,
                        PurchaseCost = cItem.Value.PurchaseCost,
                    });
                }
                else if (x is RDSValue<BountyShopArmouryItem> aItem)
                {
                    shop.ArmouryItems.Add(new()
                    {
                        ItemID = aItem.Value.ID,
                        PurchaseCost = aItem.Value.PurchaseCost
                    });
                }
            }

            return shop;
        }

        void AddArmouryItemsTable()
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

                RDSValue<BountyShopArmouryItem> rdsItem = new RDSValue<BountyShopArmouryItem>(bsItem)
                {

                };

                table.AddEntry(rdsItem);
            }

            RootTable.AddEntry(table);
        }

        void AddCurrencyItemsTable()
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
