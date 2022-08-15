using GMServer.Caching.DataFiles.Models;
using GMServer.Models.RequestModels;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using MongoDB.Driver;
using SRC.DataFiles.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public record PrestigeRequest(string UserID, LocalGameState LocalState) : IRequest<PrestigeResponse>;

    public record PrestigeResponse();

    public class PrestigeHandler : IRequestHandler<PrestigeRequest, PrestigeResponse>
    {
        private readonly AccountStatsService _accountStats;
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;
        private readonly PrestigeService _prestige;
        private readonly IBountiesService _bounties;
        private readonly IDataFileCache _dataFiles;
        public PrestigeHandler(
            ArtefactsService artefacts,
            CurrenciesService currencies,
            PrestigeService prestige,
            IBountiesService bounties,
            AccountStatsService accountStats,
            IDataFileCache dataFiles)
        {
            _accountStats = accountStats;
            _artefacts = artefacts;
            _currencies = currencies;
            _prestige = prestige;
            _bounties = bounties;
            _dataFiles = dataFiles;
        }

        public async Task<PrestigeResponse> Handle(PrestigeRequest request, CancellationToken cancellationToken)
        {
            return await HandleRequest(request);
        }

        private async Task<(List<UserArtefact>, UserBounties)> LoadDataFromMongo(string uid)
        {
            /* Load user values */
            var userArtefactsTask = _artefacts.GetUserArtefactsAsync(uid);
            var userBountiesTask = _bounties.GetUserBountiesAsync(uid);

            await Task.WhenAll(userArtefactsTask, userBountiesTask);

            /* Task results */
            var userArtefacts = userArtefactsTask.Result;
            var userBounties = userBountiesTask.Result;

            return (userArtefacts, userBounties);
        }

        private async Task<PrestigeResponse> HandleRequest(PrestigeRequest request)
        {
            /* Pull values from request */
            int prestigeStage = request.LocalState.GameState.Stage;

            (var userArtefacts, var userBounties) = await LoadDataFromMongo(request.UserID);

            ResolvedBonuses bonuses = ResolvedBonuses.Create(userArtefacts, _dataFiles.Artefacts);

            /* Calculate rewards */
            var points = bonuses.PrestigePointsAtStage(prestigeStage);         // Prestige points earned
            var defeatedBountyIds = GetBouniesDefeated(prestigeStage);                    // Already unlocked bounties which we have defeated this prestige
            var unlockedBounties = GetNewUnlockedBounties(prestigeStage, userBounties);  // Bounties which have been unlocked

            /* Reward database updates */
            List<Task> updateTasks = new()
            {
                _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = points }),
                _accountStats.UpdateUserLifetimeStatsAsync(request.UserID, new() { HighestPrestigeStage = prestigeStage, TotalPrestiges = 1 }),
                _prestige.InsertPrestigeLogAsync(new(request.UserID, prestigeStage, DateTime.UtcNow, points))
            };

            if (unlockedBounties.Count > 0)  // Insert newly unlocked bounties
                updateTasks.Add(_bounties.InsertBountiesAsync(request.UserID, unlockedBounties));

            if (defeatedBountyIds.Count > 0)  // Increment the 'NumDefeats' value used to calculate levels
                updateTasks.Add(_bounties.IncrementBountyDefeatsAsync(request.UserID, defeatedBountyIds));

            await Task.WhenAll(updateTasks);

            return new PrestigeResponse();
        }

        private List<int> GetBouniesDefeated(int prestigeStage)
        {
            return _dataFiles.Bounties.Bounties.Where(b => prestigeStage > b.UnlockStage).Select(x => x.BountyID).ToList();
        }

        private List<UserBounty> GetNewUnlockedBounties(int stage, UserBounties userBounties)
        {
            BountiesDataFile datafile = _dataFiles.Bounties;

            return datafile.Bounties
                .Where(b => !userBounties.UnlockedBounties.Exists(x => x.BountyID == b.BountyID))   // Not already unlocked
                .Where(b => stage > b.UnlockStage)                                                  // Stage was completed
                .Select(b => new UserBounty(b.BountyID))
                .ToList();
        }
    }
}