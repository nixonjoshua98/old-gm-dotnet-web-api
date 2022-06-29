using GMServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public abstract class AbstractQuest
    {
        [JsonProperty(PropertyName = "QuestID")]
        public int ID;

        public QuestActionType ActionType;

        public long LongValue;
    }

    public class MercQuest : AbstractQuest
    {
        public int RewardMercID;
    }

    public class DailyQuest : AbstractQuest
    {
        public int DiamondsRewarded;
    }

    public class QuestsDataFile
    {
        public List<MercQuest> MercQuests;
        public List<DailyQuest> DailyQuests;
    }
}
