using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Mongo.Models
{
    /*
     * NB, When adding a new statistic we will need to update the service update methods to include it
     */

    public class UserAccountStats
    {
        public int TotalPrestiges = 0;
        public int HighestPrestigeStage = 0;
        public int TotalEnemiesDefeated = 0;
        public int TotalBossesDefeated = 0;
        public int TotalTaps = 0;
    }

    [BsonIgnoreExtraElements]
    public class DailyUserAccountStats : UserAccountStats
    {
        public DateTime DateTime = DateTime.UtcNow;
    }

    [BsonIgnoreExtraElements]
    public class LifetimeUserAccountStats : UserAccountStats
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;
    }
}
