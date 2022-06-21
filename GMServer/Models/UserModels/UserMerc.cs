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

        public int UpgradePoints;

        public int ExpertiseLevel;
        public long ExpertiseExp;

        public List<MercUpgradeState> Upgrades = new();
    }

    public class MercUpgradeState
    {
        public MercUpgradeID ID;
        public int Level;
    }
}