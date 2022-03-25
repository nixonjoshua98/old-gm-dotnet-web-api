using GMServer.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public abstract class UserAbstractBountyShopItem
    {
        [JsonRequired]
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public int PurchaseCost { get; set; }
    }

    public class UserBountyShopCurrenyItem : UserAbstractBountyShopItem
    {
        public CurrencyType CurrencyType;
        public int Quantity;
    }

    public class UserBountyShopArmouryItem : UserAbstractBountyShopItem
    {
        public int ItemID { get; set; }
    }

    public class BountyShopItems
    {
        public List<UserBountyShopCurrenyItem> CurrencyItems { get; set; } = new();
        public List<UserBountyShopArmouryItem> ArmouryItems { get; set; } = new();
    }

    public class UserBountyShop
    {
        public BountyShopItems ShopItems { get; set; } = new();
    }
}
