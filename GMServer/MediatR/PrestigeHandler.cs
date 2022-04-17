using GMServer.Common;
using GMServer.Extensions;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class PrestigeRequest : IRequest<PrestigeResponse>
    {
        public string UserID;
        public int PrestigeStage;
    }

    public class PrestigeResponse : AbstractResponseWithError
    {
        public GetDataFileResponse DataFiles;
        public GetUserDataResponse UserData;

        public PrestigeResponse()
        {

        }

        public PrestigeResponse(string message, int code) : base(message, code)
        {

        }
    }

    public class PrestigeHandler : IRequestHandler<PrestigeRequest, PrestigeResponse>
    {
        private readonly AccountStatsService _accountStats;
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;
        private readonly PrestigeService _prestige;
        private readonly BountiesService _bounties;

        public PrestigeHandler(ArtefactsService artefacts, CurrenciesService currencies, PrestigeService prestige, BountiesService bounties, AccountStatsService accountStats)
        {
            _accountStats = accountStats;
            _artefacts = artefacts;
            _currencies = currencies;
            _prestige = prestige;
            _bounties = bounties;
        }

        public async Task<PrestigeResponse> Handle(PrestigeRequest request, CancellationToken cancellationToken)
        {
            // Prestige points earned
            double points = await CalculatePrestigePointsAsync(request.UserID, request.PrestigeStage);

            if (points is double.NaN || points <= 0)
                return new("Invalid prestige", 400);

            // = Bounties = //
            BountiesDataFile bountiesDatafile = _bounties.GetDataFile();
            UserBounties userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            // List of already unlocked bounties which we have defeated this prestige (may be empty)
            List<int> defeatedBountyIds = GetBouniesDefeated(bountiesDatafile, request.PrestigeStage);

            // Bounties which have been unlocked (may be empty)
            List<UserBounty> unlockedBounties = GetNewUnlockedBountiesAsync(userBounties, bountiesDatafile, request.PrestigeStage);

            // Log the prestige before we start giving rewards
            await _prestige.InsertPrestigeLogAsync(new()
            {
                DateTime = DateTime.UtcNow,
                UserID = request.UserID,
                Stage = request.PrestigeStage,
                PrestigePointsGained = points,
            });

            // Update the relevant lifetime stats
            await _accountStats.UpdateUserLifetimeStatsAsync(request.UserID, new() { HighestPrestigeStage = request.PrestigeStage, TotalPrestiges = 1 });

            if (unlockedBounties.Count > 0)  // Insert newly unlocked bounties
                await _bounties.InsertBountiesAsync(request.UserID, unlockedBounties);

            if (defeatedBountyIds.Count > 0)  // Increment the 'NumDefeats' value used to calculate levels
                await _bounties.IncrementBountyDefeats(request.UserID, defeatedBountyIds);

            await _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = points });

            return new PrestigeResponse();
        }

        private List<int> GetBouniesDefeated(BountiesDataFile bountiesDataFile, int prestigeStage)
        {
            List<int> ls = new();

            foreach (var bounty in bountiesDataFile.Bounties)
            {
                if (bounty.UnlockStage < prestigeStage)
                {
                    ls.Add(bounty.BountyID);
                }
            }

            return ls;
        }

        private List<UserBounty> GetNewUnlockedBountiesAsync(UserBounties userBounties, BountiesDataFile datafile, int stage)
        {
            List<UserBounty> newBounties = new();

            foreach (var bounty in datafile.Bounties)
            {
                bool alreadyUnlocked = userBounties.UnlockedBounties.Exists(x => x.BountyID == bounty.BountyID);

                if (alreadyUnlocked || bounty.UnlockStage > stage)
                    continue;

                newBounties.Add(new()
                {
                    BountyID = bounty.BountyID
                });
            }

            return newBounties;
        }

        private async Task<double> CalculatePrestigePointsAsync(string userId, int stage)
        {
            var userArtefacts = await _artefacts.GetUserArtefactsAsync(userId);

            double baseValue = Math.Pow(Math.Ceiling((stage - 65) / 10.0f), 2.2);

            var artefactBonuses = GameFormulas.CreateArtefactBonusList(userArtefacts, _artefacts.GetDataFile());
            var resolvedBonuses = GameFormulas.CreateResolvedBonusDictionary(artefactBonuses);

            return baseValue * resolvedBonuses.Get(BonusType.MULTIPLY_PRESTIGE_BONUS, 1);
        }
    }
}