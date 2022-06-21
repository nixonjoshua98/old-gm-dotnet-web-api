using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMServer.Models.UserModels
{
    public class UserBounty
    {
        public int BountyID;
        public int Level;
        public int NumDefeats;

        public UserBounty() { }
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
        public List<int> ActiveBounties = new();
        public List<UserBounty> UnlockedBounties = new();

        public bool IsBountyActive(int id) => ActiveBounties.Any(x => x == id);
        public bool IsBountyUnlocked(int id) => UnlockedBounties.Any(x => x.BountyID == id);
    }
}
