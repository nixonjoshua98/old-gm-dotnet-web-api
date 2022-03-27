using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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

        public async Task<UserBounties> GetUserBountiesAsync(string userId)
        {
            var update = Builders<UserBounties>.Update
                .SetOnInsert(s => s.LastClaimTime, DateTime.UtcNow);

            return await _bounties.FindOneAndUpdateAsync(x => x.UserID == userId, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true });
        }

        public async Task InsertBountiesAsync(string userId, List<UserBounty> bounties)
        {
            var update = Builders<UserBounties>.Update
                .AddToSetEach(s => s.UnlockedBounties, bounties);

            await _bounties.UpdateOneAsync(x => x.UserID == userId, update, new() { IsUpsert = true });
        }

        public async Task SetActiveBountiesAsync(string userId, IEnumerable<int> bountyIds)
        {
            var update = Builders<UserBounties>.Update
                .Set(s => s.ActiveBounties, bountyIds);

            await _bounties.UpdateOneAsync(x => x.UserID == userId, update, new() { IsUpsert = true });
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
    }
}
