using GMServer.Caching.DataFiles.Models;
using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GMServer.Mongo.Models
{
    public abstract class AbstractUserBountyShopItem
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

    public class UserBSCurrencyItem : AbstractUserBountyShopItem
    {
        public CurrencyType CurrencyType;
        public int Quantity;

        public static UserBSCurrencyItem FromShopItem(int idx, BSCurrencyItem item)
        {
            return new()
            {
                ID = $"CI-{idx}",
                Quantity = item.Quantity,
                CurrencyType = item.CurrencyType,
                PurchaseCost = item.PurchaseCost,
            };
        }
    }

    public class UserBSArmouryItem : AbstractUserBountyShopItem
    {
        public int ItemID;

        public static UserBSArmouryItem FromShopItem(int idx, BSArmouryItem item)
        {
            return new()
            {
                ID = $"AI-{idx}",
                ItemID = item.ID,
                PurchaseCost = item.PurchaseCost
            };
        }
    }

    public class UserBountyShop
    {
        [JsonIgnore]
        public string Seed { get; set; }

        public List<UserBSCurrencyItem> CurrencyItems = new();
        public List<UserBSArmouryItem> ArmouryItems = new();
    }

    [BsonIgnoreExtraElements]
    public class BountyShopPurchase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public string ItemID;

        public DateTime PurchaseTime;

        public BountyShopPurchase(string userID, string itemID, DateTime purchaseTime)
        {
            UserID = userID;
            ItemID = itemID;
            PurchaseTime = purchaseTime;
        }
    }
}
