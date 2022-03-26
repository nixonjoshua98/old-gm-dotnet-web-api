using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserAccountStatsModelBase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int TotalPrestiges = 0;
        public int HighestPrestigeStage = 0;
        public int TotalEnemiesDefeated = 0;
        public int TotalBossesDefeated = 0;
        public int TotalTaps = 0;
    }

    public class DailyUserAccountStats : UserAccountStatsModelBase
    {
        public DateTime DateTime = DateTime.UtcNow;
    }

    public class UserAccountStats
    {
        public UserAccountStatsModelBase Lifetime;
        public DailyUserAccountStats Daily;
    }
}
