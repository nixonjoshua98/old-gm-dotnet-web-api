﻿using GMServer.Common;
using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class BountyLevel
    {
        public int NumDefeatsRequired;

        public float BonusValue;
    }

    public class Bounty
    {
        public int BountyID;
        public string Name;
        public int HourlyIncome;
        public BonusType BonusType;
        public int UnlockStage;
        public List<BountyLevel> Levels = new();
    }

    public class BountiesDataFile
    {
        public float MaxUnclaimedHours;

        public List<Bounty> Bounties;
    }
}
