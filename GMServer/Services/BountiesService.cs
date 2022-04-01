using GMServer.Cache;
using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class BountiesService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserBounties> _bounties;

        public BountiesService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _bounties = mongo.GetCollection<UserBounties>("Bounties");
        }

        public async Task IncrementBountyDefeats(string userId, List<int> bountyIds)
        {
            var requests = new List<UpdateOneModel<UserBounties>>();

            foreach (var bountyId in bountyIds)
            {
                var query = new UpdateOneModel<UserBounties>(
                    // Match on UserID and any bounty which matches the BountyID (will match with duplicates if present)
                    Builders<UserBounties>.Filter.Where(x => x.UserID == userId && x.UnlockedBounties.Any(x => x.BountyID == bountyId)),
                    // Increment all matched bounties
                    Builders<UserBounties>.Update.Inc(x => x.UnlockedBounties[-1].NumDefeats, 1)
                    );

                requests.Add(query);
            }

            await _bounties.BulkWriteAsync(requests);
        }

        public async Task<UserBounties> GetUserBountiesAsync(string userId)
        {
            var update = Builders<UserBounties>.Update
                .SetOnInsert(s => s.LastClaimTime, DateTime.UtcNow);

            return await _bounties.FindOneAndUpdateAsync(x => x.UserID == userId, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true });
        }

        /// <summary>
        /// Insert the bounties provided if the user dos not have them already unlocked
        /// </summary>
        public async Task InsertBountiesAsync(string userId, List<UserBounty> bounties)
        {
            var requests = new List<UpdateOneModel<UserBounties>>();

            foreach (var bounty in bounties)
            {
                var filter = UserBountyNotUnlockedFilter(userId, bounty.BountyID);

                var update = Builders<UserBounties>.Update
                    .AddToSet(s => s.UnlockedBounties, bounty);

                var query = new UpdateOneModel<UserBounties>(filter, update);

                requests.Add(query);
            }

            await _bounties.BulkWriteAsync(requests);
        }

        public async Task SetClaimTimeAsync(string userId, DateTime claimTime)
        {
            var update = Builders<UserBounties>.Update
                .Set(s => s.LastClaimTime, claimTime);

            await _bounties.UpdateOneAsync(x => x.UserID == userId, update, new() { IsUpsert = true });
        }

        public BountiesDataFile GetDataFile()
        {
            return _cache.Load<BountiesDataFile>(DataFiles.Bounties);
        }

        /// <summary>
        /// Filter for matching against a bounty which has not yet been unlocked by the user
        /// </summary>
        private FilterDefinition<UserBounties> UserBountyNotUnlockedFilter(string userId, int bountyId)
        {
            return Builders<UserBounties>.Filter.Eq(x => x.UserID, userId) &
                    Builders<UserBounties>.Filter.Not(
                        Builders<UserBounties>.Filter.ElemMatch(x => x.UnlockedBounties, x => x.BountyID == bountyId));
        }
    }
}
