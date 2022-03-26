using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserArmouryItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int ItemID { get; set; }

        public int Level { get; set; } = 0;

        public int Owned { get; set; }
    }
}
