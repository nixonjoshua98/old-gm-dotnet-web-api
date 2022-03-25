using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserMerc
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }
        public int MercID { get; set; }
    }
}
