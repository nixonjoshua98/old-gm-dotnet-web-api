using GMServer.UserModels.DataFileModels;
using System;
using System.Collections.Generic;

namespace GMServer.UserModels.UserModels
{
    public class UserQuests
    {
        public DateTime DateTime;
        public QuestsDataFile Quests;
        public List<int> CompletedDailyQuests;
        public List<int> CompletedMercQuests;
    }
}
