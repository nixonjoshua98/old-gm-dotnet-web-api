using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Models
{
    public class AuthenticationSession
    {
        public string Token { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public bool IsValid { get; set; } = true;

        public string DeviceID { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
