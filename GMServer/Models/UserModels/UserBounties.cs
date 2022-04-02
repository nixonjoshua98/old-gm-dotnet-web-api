using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public class UserBounty
    {
        public int BountyID;
        public int Level;
        public int NumDefeats;
    }

    [BsonIgnoreExtraElements]
    public class UserBounties
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;
        public DateTime LastClaimTime;
        public List<UserBounty> UnlockedBounties = new();
    }
}
