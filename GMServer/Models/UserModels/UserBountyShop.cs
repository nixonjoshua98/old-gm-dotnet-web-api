using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public abstract class UserAbstractBountyShopItem
    {
        /// <summary>
        /// ID of the shop item (NOT the item which will be given upon purchase)
        /// </summary>
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
        public int ItemID;
    }

    public class BountyShopItems
    {
        public List<UserBountyShopCurrencyItem> CurrencyItems = new();
        public List<UserBountyShopArmouryItem> ArmouryItems = new();
    }

    [BsonIgnoreExtraElements]
    public class BountyShopPurchase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public string ItemID;

        public DateTime PurchaseTime;
    }

    public class UserBountyShop
    {
        public BountyShopItems ShopItems = new();

        public List<BountyShopPurchase> Purchases;
    }
}
