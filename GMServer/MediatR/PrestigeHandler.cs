﻿using GMServer.Calculations;
using GMServer.Common;
using GMServer.Extensions;
using GMServer.Models.DataFileModels;
using GMServer.Models.RequestModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly MercsService _mercs;
        private readonly CurrenciesService _currencies;
        private readonly PrestigeService _prestige;
        private readonly IBountiesService _bounties;

        IMongoTransactionContext _trans;

        public PrestigeHandler(
            ArtefactsService artefacts,
            CurrenciesService currencies,
            PrestigeService prestige,
            IBountiesService bounties,
            AccountStatsService accountStats,
            MercsService mercs,
            IMongoTransactionContext mongo)
        {
            _accountStats = accountStats;
            _artefacts = artefacts;
            _currencies = currencies;
            _prestige = prestige;
            _bounties = bounties;
            _mercs = mercs;
            _trans = mongo;
        }

        public async Task<PrestigeResponse> Handle(PrestigeRequest request, CancellationToken cancellationToken)
        {
            return await _trans.RunInTransaction((session) => HandleRequest(request));
        }

        public async Task<PrestigeResponse> HandleRequest(PrestigeRequest request)
        {
            /* Pull values from request */
            int prestigeStage   = request.LocalState.GameState.Stage;
            var mercStates      = request.LocalState.MercStates;

            /* Load user values */
            var userArtefactsTask   = _artefacts.GetUserArtefactsAsync(request.UserID);
            var userMercsTask       = _mercs.GetUserMercsAsync(request.UserID);
            var userBountiesTask    = _bounties.GetUserBountiesAsync(request.UserID);

            await Task.WhenAll(userArtefactsTask, userMercsTask, userBountiesTask);

            /* Task results */
            var userArtefacts   = userArtefactsTask.Result;
            var userMercs       = userMercsTask.Result;
            var userBounties    = userBountiesTask.Result;

            /* Calculate rewards */
            var points              = CalculatePrestigePoints(userArtefacts, prestigeStage);                // Prestige points earned
            var mercUpdateModels    = MercCalculations.GetMercUpdateModels(mercStates, userMercs);          // Calculate merc changes
            var defeatedBountyIds   = GetBouniesDefeated(prestigeStage);                                    // Already unlocked bounties which we have defeated this prestige
            var unlockedBounties    = GetNewUnlockedBounties(request.UserID, prestigeStage, userBounties);  // Bounties which have been unlocked

            /* Reward database updates */
            List<Task> updateTasks = new()
            {
                _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = points }),
                _accountStats.UpdateUserLifetimeStatsAsync(request.UserID, new() { HighestPrestigeStage = prestigeStage, TotalPrestiges = 1 })
            };

            updateTasks.Add(_prestige.InsertPrestigeLogAsync(new(request.UserID, prestigeStage, DateTime.UtcNow, points)));

            if (mercUpdateModels.Count > 0) // Update merc levels etc.
                updateTasks.Add(_mercs.UpdateMercs(request.UserID, mercUpdateModels));

            if (unlockedBounties.Count > 0)  // Insert newly unlocked bounties
                updateTasks.Add(_bounties.InsertBountiesAsync(request.UserID, unlockedBounties));

            if (defeatedBountyIds.Count > 0)  // Increment the 'NumDefeats' value used to calculate levels
                updateTasks.Add(_bounties.IncrementBountyDefeatsAsync(request.UserID, defeatedBountyIds));

            await Task.WhenAll(updateTasks);
            
            return new PrestigeResponse();
        }

        private List<int> GetBouniesDefeated(int prestigeStage)
        {
            return _bounties.GetDataFile().Bounties.Where(b => prestigeStage > b.UnlockStage).Select(x => x.BountyID).ToList();
        }

        private List<UserBounty> GetNewUnlockedBounties(string userId, int stage, UserBounties userBounties)
        {
            BountiesDataFile datafile = _bounties.GetDataFile();

            return  datafile.Bounties
                .Where(b => !userBounties.UnlockedBounties.Exists(x => x.BountyID == b.BountyID))   // Not already unlocked
                .Where(b => stage > b.UnlockStage)                                                  // Stage was completed
                .Select(b => new UserBounty(b.BountyID))
                .ToList();
        }

        private double CalculatePrestigePoints(List<UserArtefact> bounties, int stage)
        {
            double baseValue    = GameFormulas.PrestigePointsBase(stage);
            var artefactBonuses = GameFormulas.CreateArtefactBonusList(bounties, _artefacts.GetDataFile());
            var resolvedBonuses = GameFormulas.CreateResolvedBonusDictionary(artefactBonuses);

            return baseValue * resolvedBonuses.Get(BonusType.MULTIPLY_PRESTIGE_BONUS, 1);
        }
    }
}