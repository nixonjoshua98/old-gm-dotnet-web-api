using GMServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.UserModels.DataFileModels
{
    public class MercQuest
    {
        [JsonProperty(PropertyName = "QuestID")]
        public int ID { get; set; }
        public int RequiredStage { get; set; }
        public int RewardMercID { get; set; }
    }

    public class DailyQuest
    {
        [JsonProperty(PropertyName = "QuestID")]
        public int ID { get; set; }
        public int RequiredStage { get; set; }
        public int RewardMercID { get; set; }
        public QuestActionType ActionType { get; set; }
        public long LongValue { get; set; } = -1;
        public int DiamondsRewarded { get; set; }
    }

    public class QuestsDataFile
    {
        public List<MercQuest> MercQuests { get; set; }
        public List<DailyQuest> DailyQuests { get; set; }
    }
}
