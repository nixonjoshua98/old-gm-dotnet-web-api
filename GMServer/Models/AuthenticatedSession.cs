using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models
{
    [BsonIgnoreExtraElements]
    public class AuthenticatedSession
    {
        public string Token;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public bool IsValid = true;
    }
}
