﻿using SRC.Caching.DataFiles.Models;
using System;
using System.Collections.Generic;

namespace SRC.Mongo.Models
{
    public class UserQuests
    {
        public DateTime DateTime;
        public QuestsDataFile Quests;
        public List<int> CompletedDailyQuests;
        public List<int> CompletedMercQuests;
    }
}
