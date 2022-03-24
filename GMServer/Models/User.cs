using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models
{
    public class User
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        public string DeviceID { get; set; }
    }
}
