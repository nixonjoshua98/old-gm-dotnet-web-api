using GMServer.UserModels.DataFileModels;
using GMServer.UserModels.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

            return await _bounties.FindOneAndUpdateAsync(x => x.UserID == userId, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true});
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
