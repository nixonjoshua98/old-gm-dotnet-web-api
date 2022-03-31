using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public class UserBounty
    {
        public int BountyID;
        public int NumDefeats = 0;
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
