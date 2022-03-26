using System.Collections.Generic;

namespace GMServer.Models.RequestModels
{
    public class UpdateActiveBountiesModel
    {
        public HashSet<int> BountyIds; // Remove the chance of having duplicate bounties
    }
}
