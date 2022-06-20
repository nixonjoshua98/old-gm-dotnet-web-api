using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserMerc
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int MercID;

        public long CurrentXP;
        public long TotalXP;
    }
}
