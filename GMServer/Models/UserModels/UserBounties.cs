using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.UserModels.UserModels
{
    public class UserBounty
    {
        public int BountyID { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class UserBounties
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;
        public DateTime LastClaimTime;
        public List<int> ActiveBounties = new();
        public List<UserBounty> UnlockedBounties = new();
    }
}
