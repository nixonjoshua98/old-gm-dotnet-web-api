using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public abstract class UserAbstractBountyShopItem
    {
        public string ID;
        public int PurchaseCost;
    }

    public class UserBountyShopCurrencyItem : UserAbstractBountyShopItem
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
        public List<UserBountyShopCurrencyItem> CurrencyItems { get; set; } = new();
        public List<UserBountyShopArmouryItem> ArmouryItems { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class BountyShopPurchase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public string ItemID { get; set; }

        public DateTime PurchaseTime { get; set; }
    }

    public class UserBountyShop
    {
        public BountyShopItems ShopItems { get; set; } = new();

        public List<BountyShopPurchase> Purchases { get; set; }
    }
}
