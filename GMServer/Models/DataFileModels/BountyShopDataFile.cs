using GMServer.Common;
using GMServer.LootTable;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GMServer.Models.DataFileModels
{
    public class BountyShopDataFile
    {
        public List<BountyShopConfig> ShopItemConfigs;

        public BountyShopConfig GetConfig(long hourlyIncome)
        {
            return ShopItemConfigs.OrderBy(x => x.HourlyIncomeRequired).Where(x => hourlyIncome >= x.HourlyIncomeRequired).Last();
        }

        public BountyShopConfig GetLevelConfig(int level)
        {
            return ShopItemConfigs.Where(x => level == x.Level).Last();
        }
    }

    public class BountyShopConfig
    {
        public int Level;
        public long HourlyIncomeRequired;

        public BSCurrencyItems CurrencyItems;
        public BSArmouryItems ArmouryItems;
    }

    public class BSCurrencyItem : LootItem
    {
        public CurrencyType CurrencyType;
        public int PurchaseCost;
        public int Quantity;
    }

    public class BSCurrencyItems : LootItem
    {
        public List<BSCurrencyItem> Items;
    }

    public class BSArmouryItems : LootItem
    {
        public List<BSArmouryItemGradeConfig> ItemGrades;
    }

    public class BSArmouryItem : LootItem
    {
        [JsonProperty(PropertyName = "ItemID")]
        public int ID;

        [JsonRequired]
        public int PurchaseCost;
    }

    public class BSArmouryItemGradeConfig : LootItem
    {
        [JsonRequired]
        public ItemGradeID ItemGrade;

        [JsonRequired]
        public int PurchaseCost;
    }
}
