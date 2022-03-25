using System.Collections.Generic;

namespace GMServer.UserModels.DataFileModels
{
    public class Bounty
    {
        public int BountyID { get; set; }
        public int HourlyIncome { get; set; }
        public int UnlockStage { get; set; }
    }

    public class BountiesDataFile
    {
        public float MaxUnclaimedHours { get; set; }
        public int MaxActiveBounties { get; set; }

        public List<Bounty> Bounties;
    }
}
