using SRC.Common;
using System.Collections.Generic;

namespace SRC.Caching.DataFiles.Models
{
    public class BountyLevel
    {
        public int Level;
        public int KillsRequired;
        public float BonusValue;
    }

    public class Bounty
    {
        public int BountyID;
        public string Name;
        public int Tier;
        public int PointsPerHour;
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
