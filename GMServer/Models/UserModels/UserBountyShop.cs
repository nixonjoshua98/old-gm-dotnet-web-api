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

    [BsonIgnoreExtraElements]
    public class UserBountyShopState
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public DateTime LastUpdated;
        public string Seed;
        public int Level;
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
}
