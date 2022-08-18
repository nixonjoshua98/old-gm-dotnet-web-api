using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRC.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class BountyShopModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int GameDayNumber;

        public List<BountyShopPurchaseModel> Purchases = new();

        public BountyShopPurchaseModel GetPurchase(BountyShopItemType itemType, string itemId)
        {
            return Purchases.FirstOrDefault(p => p.ItemID == itemId && p.ItemType == itemType);
        }
    }

    public class BountyShopPurchaseModel
    {
        public string ItemID;
        public BountyShopItemType ItemType;

        public BountyShopPurchaseModel(string itemID, BountyShopItemType itemType)
        {
            ItemID = itemID;
            ItemType = itemType;
        }
    }










    public abstract class AbstractUserBountyShopItem
    {
        public string ID;
        public int PurchaseCost;
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
                ItemID = item.ItemID,
                PurchaseCost = item.PurchaseCost
            };
        }
    }

    public class UserBountyShop
    {
        public List<UserBSCurrencyItem> CurrencyItems = new();
        public List<UserBSArmouryItem> ArmouryItems = new();
    }

    [BsonIgnoreExtraElements]
    public class BountyShopPurchase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public string ItemID;

        public BountyShopItemType ItemType;

        public int GameDayNumber;

        public DateTime PurchaseTime;
    }
}
