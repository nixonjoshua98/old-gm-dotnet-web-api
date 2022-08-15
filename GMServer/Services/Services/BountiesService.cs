using GMServer.Mongo.Models;
using GMServer.Mongo.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public interface IBountiesService
    {
        Task AddActiveBountyAsync(string userId, int bountyId);
        Task<UserBounties> GetUserBountiesAsync(string userId);
        Task<UserBounty> GetUserBountyAsync(string userId, int bountyId);
        Task IncrementBountyDefeatsAsync(string userId, List<int> bountyIds);
        Task IncrementBountyLevelAsync(string userId, int bountyId, int levels);
        Task InsertBountiesAsync(string userId, List<UserBounty> bounties);
        Task RemoveActiveBountyAsync(string userId, int bountyId);
        Task UpdateUserAsync(string userId, Func<UpdateDefinitionBuilder<UserBounties>, UpdateDefinition<UserBounties>> update);
    }

    public class BountiesService : IBountiesService
    {
        private readonly IBountiesRepository _bounties;

        public BountiesService(IBountiesRepository bounties)
        {
            _bounties = bounties;
        }

        public async Task IncrementBountyDefeatsAsync(string userId, List<int> bountyIds)
        {
            var requests = bountyIds.Select(bountyId =>
            {
                var filter = _bounties.BountyFilter(userId, bountyId);
                var update = _bounties.Update.Inc(x => x.UnlockedBounties[-1].NumDefeats, 1);

                return new UpdateOneModel<UserBounties>(filter, update);
            }).ToList();

            await _bounties.BulkWriteAsync(requests);
        }

        public async Task RemoveActiveBountyAsync(string userId, int bountyId)
        {
            await _bounties.UpdateOneAsync(x => x.UserID == userId, upd => upd.Pull(x => x.ActiveBounties, bountyId));
        }

        public async Task AddActiveBountyAsync(string userId, int bountyId)
        {
            await _bounties.UpdateOneAsync(x => x.UserID == userId, upd => upd.AddToSet(x => x.ActiveBounties, bountyId));
        }

        public async Task IncrementBountyLevelAsync(string userId, int bountyId, int levels)
        {
            var filter = _bounties.BountyFilter(userId, bountyId);
            var update = _bounties.Update.Inc(x => x.UnlockedBounties[-1].Level, levels);

            await _bounties.UpdateOneAsync(filter, update);
        }

        public async Task<UserBounties> GetUserBountiesAsync(string userId)
        {
            var update = Builders<UserBounties>.Update
                .SetOnInsert(s => s.LastClaimTime, DateTime.UtcNow);

            return await _bounties.FindOneAndUpdateAsync(x => x.UserID == userId, update, isUpsert: true);
        }

        public async Task<UserBounty> GetUserBountyAsync(string userId, int bountyId)
        {
            var result = await GetUserBountiesAsync(userId);

            return result.UnlockedBounties.FirstOrDefault(x => x.BountyID == bountyId);
        }

        /// <summary>
        /// Insert the bounties provided if the user dos not have them already unlocked
        /// </summary>
        public async Task InsertBountiesAsync(string userId, List<UserBounty> bounties)
        {
            var requests = new List<WriteModel<UserBounties>>();

            foreach (var bounty in bounties)
            {
                var filter = _bounties.LockedBountyFilter(userId, bounty.BountyID);
                var update = _bounties.Update.AddToSet(s => s.UnlockedBounties, bounty);

                var query = new UpdateOneModel<UserBounties>(filter, update);

                requests.Add(query);
            }

            await _bounties.BulkWriteAsync(requests);
        }

        public async Task UpdateUserAsync(string userId, Func<UpdateDefinitionBuilder<UserBounties>, UpdateDefinition<UserBounties>> update)
        {
            await _bounties.UpdateOneAsync(doc => doc.UserID == userId, update);
        }
    }
}
