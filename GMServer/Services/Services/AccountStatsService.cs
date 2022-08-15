﻿using GMServer.Mongo.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class AccountStatsService
    {
        private readonly IMongoCollection<LifetimeUserAccountStats> _lifetime;

        public AccountStatsService(IMongoDatabase mongo)
        {
            _lifetime = mongo.GetCollection<LifetimeUserAccountStats>("LifetimeUserStats");
        }

        public async Task<LifetimeUserAccountStats> GetUserLifetimeStatsAsync(string userId)
        {
            return await _lifetime.Find(x => x.UserID == userId).FirstOrDefaultAsync() ?? new() { UserID = userId };
        }

        public async Task<LifetimeUserAccountStats> UpdateLifetimeStatsWithLocalChangesAsync(string userId, UserAccountStats incr)
        {
            var filter = Builders<LifetimeUserAccountStats>.Filter.Eq(x => x.UserID, userId);
            var update = Builders<LifetimeUserAccountStats>.Update
                .Inc(x => x.TotalEnemiesDefeated, incr.TotalEnemiesDefeated)
                .Inc(x => x.TotalBossesDefeated, incr.TotalBossesDefeated)
                .Inc(x => x.TotalTaps, incr.TotalTaps);

            return await _lifetime.FindOneAndUpdateAsync(filter, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true });
        }

        public async Task<LifetimeUserAccountStats> UpdateUserLifetimeStatsAsync(string userId, UserAccountStats incr)
        {
            var update = Builders<LifetimeUserAccountStats>.Update
                .Inc(x => x.TotalPrestiges, incr.TotalPrestiges)
                .Max(x => x.HighestPrestigeStage, incr.HighestPrestigeStage);

            return await _lifetime.FindOneAndUpdateAsync(x => x.UserID == userId, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true });
        }
    }
}
