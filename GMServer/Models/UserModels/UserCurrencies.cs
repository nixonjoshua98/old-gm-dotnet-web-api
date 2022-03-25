using GMServer.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserCurrencies
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        [MongoIncrement]
        public int Diamonds { get; set; } = 0;

        [MongoIncrement]
        public long PrestigePoints { get; set; } = 0;

        [MongoIncrement]
        public long BountyPoints { get; set; } = 0;

        [MongoIncrement]
        public long ArmouryPoints { get; set; } = 0;
    }
}