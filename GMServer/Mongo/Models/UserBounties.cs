using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SRC.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class UserBounty
    {
        public int BountyID;
        public int Level = 1;
        public int CurrentKillCount;

        public UserBounty(int id)
        {
            BountyID = id;
        }
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
