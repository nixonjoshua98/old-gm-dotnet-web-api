using GMServer.Context;
using GMServer.Models.UserModels;
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

        public async Task<LifetimeUserAccountStats> UpdateUserLifetimeStatsAsync(string userId, UserAccountStatsModelBase incr)
        {
            var filter = Builders<LifetimeUserAccountStats>.Filter.Eq(x => x.UserID, userId);
            var update = Builders<LifetimeUserAccountStats>.Update
                .Inc(x => x.TotalPrestiges, incr.TotalPrestiges)
                .Max(x => x.HighestPrestigeStage, incr.HighestPrestigeStage)
                .Inc(x => x.TotalEnemiesDefeated, incr.TotalEnemiesDefeated)
                .Inc(x => x.TotalBossesDefeated, incr.TotalBossesDefeated)
                .Inc(x => x.TotalTaps, incr.TotalTaps);

            return await _lifetime.FindOneAndUpdateAsync(filter, update, new() { ReturnDocument = ReturnDocument.After, IsUpsert = true });
        }

        public Task<DailyUserAccountStats> GetUserDailyStatsAsync(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return Task.FromResult(new DailyUserAccountStats());
        }
    }
}
