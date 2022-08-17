using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Mongo.Models
{
    public class User
    {
        [BsonId]
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; }

        public string DeviceID;
        public string AccessToken;
    }
}
