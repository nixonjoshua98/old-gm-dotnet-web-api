using GMServer.MediatR;
using GMServer.Models.UserModels;
using System.Collections.Generic;

namespace GMServer.Models
{
    public class AggregatedUserData
    {
        public List<UserArtefact> Artefacts { get; set; }
        public List<UserArmouryItem> ArmouryItems { get; set; }
        public UserCurrencies Currencies { get; set; }
        public List<UserMerc> UnlockedMercs { get; set; }
        public UserBounties Bounties { get; set; }
        public GetUserQuestsResponse Quests { get; set; }
    }
}
