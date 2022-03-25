using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
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

        public BountiesDataFile GetDataFile()
        {
            return _cache.Load<BountiesDataFile>(DataFiles.Bounties);
        }
    }
}
