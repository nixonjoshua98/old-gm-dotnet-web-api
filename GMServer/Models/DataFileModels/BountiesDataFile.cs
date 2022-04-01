using System.Collections.Generic;

namespace GMServer.Models.DataFileModels
{
    public class Bounty
    {
        public int BountyID;
        public string Name;
        public int HourlyIncome;
        public int UnlockStage;
    }

    public class BountiesDataFile
    {
        public float MaxUnclaimedHours;

        public List<Bounty> Bounties;
    }
}
