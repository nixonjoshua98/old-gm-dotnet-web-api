using GMServer.Common;
using GMServer.LootTable;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class BountyShopCurrencyItem : LootItem
    {
        [JsonRequired]
        public CurrencyType CurrencyType;

        [JsonRequired]
        public int PurchaseCost;

        [JsonRequired]
        public int Quantity;
    }

    public class BountyShopArmouryItem : LootItem
    {
        [JsonProperty(PropertyName = "ItemID", Required = Required.Always)]
        public int ID;

        [JsonRequired]
        public int PurchaseCost;
    }

    public class BountyShopCurrencyItemsDataFile : LootItem
    {
        public List<BountyShopCurrencyItem> Items;
    }

    public class BountyShopArmouryItemsDataFile : LootItem
    {
        public int PurchaseCost;
    }

    public class BountyShopDataFile
    {
        public BountyShopCurrencyItemsDataFile CurrencyItems;
        public BountyShopArmouryItemsDataFile ArmouryItems;
    }
}
