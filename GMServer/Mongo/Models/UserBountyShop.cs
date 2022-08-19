using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Enums;
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
}
