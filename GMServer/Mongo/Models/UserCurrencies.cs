using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SRC.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class UserCurrencies
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int Gemstones;
        public double PrestigePoints;
        public long BountyPoints;
        public long ArmouryPoints;
    }
}