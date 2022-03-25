using GMServer.Common;
using GMServer.LootTable;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class BountyShopCurrencyItem : LootItem
    {
        [JsonRequired]
        public CurrencyType CurrencyType { get; set; }

        [JsonRequired]
        public int PurchaseCost { get; set; }

        [JsonRequired]
        public int Quantity { get; set; }
    }

    public class BountyShopArmouryItem : LootItem
    {
        [JsonProperty(PropertyName = "ItemID", Required = Required.Always)]
        public int ID { get; set; }

        [JsonRequired]
        public int PurchaseCost { get; set; }
    }

    public class BountyShopCurrencyItemsDataFile : LootItem
    {
        public List<BountyShopCurrencyItem> Items { get; set; }
    }

    public class BountyShopArmouryItemsDataFile : LootItem
    {
        public int PurchaseCost { get; set; }
    }

    public class BountyShopDataFile
    {
        public BountyShopCurrencyItemsDataFile CurrencyItems { get; set; }
        public BountyShopArmouryItemsDataFile ArmouryItems { get; set; }
    }
}
