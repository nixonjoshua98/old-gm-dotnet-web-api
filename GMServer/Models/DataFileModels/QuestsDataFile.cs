using GMServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class MercQuest
    {
        [JsonProperty(PropertyName = "QuestID")]
        public int ID;
        public int RequiredStage;
        public int RewardMercID;
    }

    public class DailyQuest
    {
        [JsonProperty(PropertyName = "QuestID")]
        public int ID;
        public QuestActionType ActionType;
        public long LongValue = -1;
        public int DiamondsRewarded;
    }

    public class QuestsDataFile
    {
        public List<MercQuest> MercQuests;
        public List<DailyQuest> DailyQuests;
    }
}
