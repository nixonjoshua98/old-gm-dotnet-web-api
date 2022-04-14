using GMServer.Common;
using GMServer.LootTable;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GMServer.Models.DataFileModels
{
    public class BountyShopDataFile
    {
        public List<BountyShopLootTableConfig> ShopItemConfigs;

        public BountyShopLootTableConfig GetConfig(long hourlyIncome)
        {
            return ShopItemConfigs.Where(x => hourlyIncome >= x.HourlyIncomeRequired).Last();
        }

        public BountyShopLootTableConfig GetLevelConfig(int level)
        {
            return ShopItemConfigs.Where(x => level == x.Level).Last();
        }

        [OnDeserialized]
        private void OnSerialized(StreamingContext context)
        {
            ShopItemConfigs.Sort((x, y) => x.HourlyIncomeRequired.CompareTo(y.HourlyIncomeRequired));
        }
    }

    public class BountyShopLootTableConfig
    {
        public int Level;
        public long HourlyIncomeRequired;

        public BountyShopCurrencyItems CurrencyItems;
        public BountyShopArmouryItems ArmouryItems;
    }


    // Currency Items

    public class BountyShopCurrencyItem : LootItem
    {
        [JsonRequired]
        public CurrencyType CurrencyType;

        [JsonRequired]
        public int PurchaseCost;

        [JsonRequired]
        public int Quantity;
    }

    public class BountyShopCurrencyItems : LootItem
    {
        public List<BountyShopCurrencyItem> Items;
    }

    // Armoury Items

    public class BountyShopArmouryItems : LootItem
    {
        public List<BountyShopArmouryItemGradeLootItem> ItemGrades;
    }

    public class BountyShopArmouryItem : LootItem
    {
        [JsonProperty(PropertyName = "ItemID", Required = Required.Always)]
        public int ID;

        [JsonRequired]
        public int PurchaseCost;
    }

    public class BountyShopArmouryItemGradeLootItem : LootItem
    {
        [JsonRequired]
        public ItemGrade ItemGrade;

        [JsonRequired]
        public int PurchaseCost;
    }
}
