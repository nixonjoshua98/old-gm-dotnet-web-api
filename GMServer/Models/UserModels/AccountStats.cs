using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Models.UserModels
{
    /*
     * NB, When adding a new statistic we will need to update the service update methods to include it
     */

    public class UserAccountStatsModelBase
    {
        public int TotalPrestiges = 0;
        public int HighestPrestigeStage = 0;
        public int TotalEnemiesDefeated = 0;
        public int TotalBossesDefeated = 0;
        public int TotalTaps = 0;
    }

    [BsonIgnoreExtraElements]
    public class DailyUserAccountStats : UserAccountStatsModelBase
    {
        public DateTime DateTime = DateTime.UtcNow;
    }

    [BsonIgnoreExtraElements]
    public class LifetimeUserAccountStats : UserAccountStatsModelBase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;
    }
}
