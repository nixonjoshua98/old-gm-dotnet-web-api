using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels
{
    [BsonIgnoreExtraElements]
    public class AuthenticatedSession
    {
        public string Token { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public bool IsValid { get; set; } = true;
    }
}
