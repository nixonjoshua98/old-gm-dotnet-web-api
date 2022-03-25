using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class AccountStatsModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int TotalPrestiges = 0;
        public int HighestPrestigeStage = 0;
        public int TotalEnemiesDefeated = 0;
        public int TotalBossesDefeated = 0;
        public int TotalTaps = 0;
    }

    public class DailyAccountStats : AccountStatsModel
    {
        public DateTime DateTime = DateTime.UtcNow;
    }

    public class AccountStats
    {
        public AccountStatsModel Lifetime;
        public DailyAccountStats Daily;
    }
}
