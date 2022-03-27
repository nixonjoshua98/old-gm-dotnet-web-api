using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class Bounty
    {
        public int BountyID;
        public int HourlyIncome;
        public int UnlockStage;
    }

    public class BountiesDataFile
    {
        public float MaxUnclaimedHours;
        public int MaxActiveBounties;

        public List<Bounty> Bounties;
    }
}
