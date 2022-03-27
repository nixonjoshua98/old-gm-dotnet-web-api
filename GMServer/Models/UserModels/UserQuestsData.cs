using GMServer.Models.DataFileModels;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    public class UserQuests
    {
        public DateTime DateTime;
        public QuestsDataFile Quests;
        public List<int> CompletedDailyQuests;
        public List<int> CompletedMercQuests;
    }
}
