using GMServer.Common;
using System.Collections.Generic;

namespace GMServer.Caching.DataFiles.Models
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
        public string Description = "This enemy prefers to not share his food";
        public List<BountyLevel> Levels = new();
    }

    public class BountiesDataFile
    {
        public float MaxUnclaimedHours;
        public int MaxActiveBounties = 4;

        public List<Bounty> Bounties;
    }
}
