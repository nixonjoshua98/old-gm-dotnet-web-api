using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserMerc
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int MercID;
    }
}