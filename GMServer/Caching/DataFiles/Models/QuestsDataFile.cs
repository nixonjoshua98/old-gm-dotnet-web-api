using Newtonsoft.Json;
using SRC.Common;
using System.Collections.Generic;

namespace SRC.Caching.DataFiles.Models
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
