using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class UserArmouryItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int ItemID;

        public int Level;

        public int Owned;
    }
}
