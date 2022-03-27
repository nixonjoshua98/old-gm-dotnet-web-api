using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserArmouryItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int ItemID;

        public int Level = 0;

        public int Owned;
    }
}
