using GMServer.Common;
using GMServer.Extensions;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using GMServer.Models.RequestModels;
using MongoDB.Driver;

namespace GMServer.MediatR
{
    public class PrestigeRequest : IRequest<PrestigeResponse>
    {
        public string UserID;
        public LocalGameState LocalState;
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
        private readonly UnitService _units;
        private readonly CurrenciesService _currencies;
        private readonly PrestigeService _prestige;
        private readonly IBountiesService _bounties;

        public PrestigeHandler(
            ArtefactsService artefacts,
            CurrenciesService currencies,
            PrestigeService prestige,
            IBountiesService bounties,
            AccountStatsService accountStats,
            UnitService units)
        {
            _accountStats = accountStats;
            _artefacts = artefacts;
            _currencies = currencies;
            _prestige = prestige;
            _bounties = bounties;
            _units = units;
        }

        public async Task<PrestigeResponse> Handle(PrestigeRequest request, CancellationToken cancellationToken)
        {
            /* Pull values from request */
            int prestigeStage = request.LocalState.GameState.Stage;

            var userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            // Prestige points earned
            double points = CalculatePrestigePoints(userArtefacts, prestigeStage);

            if (points is double.NaN || points <= 0)
                return new("Invalid prestige", 400);

            // Calculate merc xp gained
            var unitXPGained = GetUnitsXPEarned(request.LocalState.UnitStates);

            // List of already unlocked bounties which we have defeated this prestige (may be empty)
            List<int> defeatedBountyIds = GetBouniesDefeated(prestigeStage);

            // Bounties which have been unlocked (may be empty)
            List<UserBounty> unlockedBounties = await GetNewUnlockedBountiesAsync(request.UserID, prestigeStage);

            // Increment earned currencies
            await _currencies.IncrementAsync(request.UserID, new()
            {
                PrestigePoints = points
            });

            // Update relevant account stats
            await _accountStats.UpdateUserLifetimeStatsAsync(request.UserID, new()
            {
                HighestPrestigeStage = prestigeStage,
                TotalPrestiges = 1
            });

            await _prestige.InsertPrestigeLogAsync(new (request.UserID, prestigeStage, DateTime.UtcNow, points));

            if (unitXPGained.Count > 0) // Update Unit XP
                await _units.UpdateUnitXP(request.UserID, unitXPGained);

            if (unlockedBounties.Count > 0)  // Insert newly unlocked bounties
                await _bounties.InsertBountiesAsync(request.UserID, unlockedBounties);

            if (defeatedBountyIds.Count > 0)  // Increment the 'NumDefeats' value used to calculate levels
                await _bounties.IncrementBountyDefeatsAsync(request.UserID, defeatedBountyIds);
            
            return new PrestigeResponse();
        }

        private List<int> GetBouniesDefeated(int prestigeStage)
        {
            return _bounties.GetDataFile().Bounties.Where(b => prestigeStage > b.UnlockStage).Select(x => x.BountyID).ToList();
        }

        private async Task<List<UserBounty>> GetNewUnlockedBountiesAsync(string userId, int stage)
        {
            UserBounties userBounties = await _bounties.GetUserBountiesAsync(userId);

            BountiesDataFile datafile = _bounties.GetDataFile();

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

        private double CalculatePrestigePoints(List<UserArtefact> bounties, int stage)
        {
            double baseValue    = GameFormulas.BasePrestigePoints(stage);
            var artefactBonuses = GameFormulas.CreateArtefactBonusList(bounties, _artefacts.GetDataFile());
            var resolvedBonuses = GameFormulas.CreateResolvedBonusDictionary(artefactBonuses);

            return baseValue * resolvedBonuses.Get(BonusType.MULTIPLY_PRESTIGE_BONUS, 1);
        }

        private Dictionary<int, long> GetUnitsXPEarned(List<LocalUserUnitState> units)
        {
            var result = new Dictionary<int, long>();

            foreach (var unit in units)
            {
                result[unit.ID] = GameFormulas.MercXPEarned(unit.EnemiesDefeatedSincePrestige);
            }

            return result;
        }
    }
}