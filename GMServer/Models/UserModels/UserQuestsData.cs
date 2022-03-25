using GMServer.UserModels.DataFileModels;
using System;
using System.Collections.Generic;

namespace GMServer.UserModels.UserModels
{
    public class UserQuests
    {
        public DateTime DateTime;
        public QuestsDataFile Quests { get; set; }
        public List<int> CompletedDailyQuests { get; set; }
        public List<int> CompletedMercQuests { get; set; }
    }
}
