using GMServer.Context;
using GMServer.Models.UserModels;
using GMServer.Services;
using GMServer.UserModels.UserModels;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class GetUserDataRequest : IRequest<GetUserDataResponse>
    {
        public string UserID { get; set; }
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh { get; set; }
    }

    public class GetUserDataResponse
    {
        public List<UserArtefact> Artefacts;
        public List<UserArmouryItem> ArmouryItems;
        public UserCurrencies Currencies;
        public List<UserMerc> UnlockedMercs;
        public UserBounties Bounties;
        public UserQuests Quests;
        public UserBountyShop BountyShop;
        public UserAccountStats UserStats;
    }

    public class GetUserDataHandler : IRequestHandler<GetUserDataRequest, GetUserDataResponse>
    {
        private readonly ArtefactsService _artefacts;
        private readonly ArmouryService _armoury;
        private readonly QuestsService _quests;
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly MercService _mercs;
        private readonly AccountStatsService _stats;
        private readonly BountiesService _bounties;

        public GetUserDataHandler(
            ArtefactsService artefacts, 
            ArmouryService armoury, 
            CurrenciesService currencies, 
            MercService mercs, 
            BountiesService bounties,
            QuestsService quests,
            BountyShopService bountyshop,
            AccountStatsService stats)
        {
            _artefacts = artefacts;
            _armoury = armoury;
            _quests = quests;
            _bountyshop = bountyshop;
            _currencies = currencies;
            _mercs = mercs;
            _stats = stats;
            _bounties = bounties;
        }

        public async Task<GetUserDataResponse> Handle(GetUserDataRequest request, CancellationToken cancellationToken)
        {
            GetUserDataResponse response = new()
            {
                Artefacts = await _artefacts.GetUserArtefactsAsync(request.UserID),
                ArmouryItems = await _armoury.GetUserArmouryItemsAsync(request.UserID),
                Currencies = await _currencies.GetUserCurrenciesAsync(request.UserID),
                UnlockedMercs = await _mercs.GetUserMercsAsync(request.UserID),
                Bounties = await _bounties.GetUserBountiesAsync(request.UserID),
                Quests = await _quests.GetUserQuestsAsync(request.UserID, request.DailyRefresh),
                BountyShop = new()
                {
                    ShopItems = _bountyshop.GetUserBountyShop(request.UserID, request.DailyRefresh),
                    Purchases = await _bountyshop.GetUserDailyPurchasesAsync(request.UserID, request.DailyRefresh)
                },
                UserStats = new()
                {
                    Lifetime = await _stats.GetUserLifetimeStatsAsync(request.UserID),
                    Daily = await _stats.GetUserDailyStatsAsync(request.UserID, request.DailyRefresh)
                }
            };

            return response;
        }
    }
}
