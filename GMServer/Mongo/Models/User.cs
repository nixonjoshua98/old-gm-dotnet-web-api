using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SRC.Mongo.Models
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
