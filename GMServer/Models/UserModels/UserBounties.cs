using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public class UserBounty
    {
        public int BountyID { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class UserBounties
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }
        public DateTime LastClaimTime { get; set; }
        public List<int> ActiveBounties { get; set; } = new();
        public List<UserBounty> UnlockedBounties { get; set; } = new();
    }
}
