using GMServer.Context;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class AccountStatsService
    {
        private readonly IMongoCollection<UserAccountStatsModelBase> _lifetime;
        private readonly IMongoCollection<UserAccountStatsModelBase> _daily;

        public AccountStatsService(IMongoDatabase mongo)
        {
            _lifetime = mongo.GetCollection<UserAccountStatsModelBase>("LifetimeUserStats");
            _daily = mongo.GetCollection<UserAccountStatsModelBase>("DailyUserStats");
        }

        public async Task<UserAccountStatsModelBase> GetUserLifetimeStatsAsync(string userId)
        {
            return await _lifetime.Find(x => x.UserID == userId).FirstOrDefaultAsync() ?? new() { UserID = userId };
        }

        public Task<DailyUserAccountStats> GetUserDailyStatsAsync(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return Task.FromResult(new DailyUserAccountStats());
        }
    }
}
