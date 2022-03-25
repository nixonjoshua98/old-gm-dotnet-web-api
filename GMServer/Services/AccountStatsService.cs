using GMServer.Context;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class AccountStatsService
    {
        private readonly IMongoCollection<AccountStatsModel> _lifetime;
        private readonly IMongoCollection<AccountStatsModel> _daily;

        public AccountStatsService(IMongoDatabase mongo)
        {
            _lifetime = mongo.GetCollection<AccountStatsModel>("LifetimeUserStats");
            _daily = mongo.GetCollection<AccountStatsModel>("DailyUserStats");
        }

        public async Task<AccountStatsModel> GetUserLifetimeStatsAsync(string userId)
        {
            return await _lifetime.Find(x => x.UserID == userId).FirstOrDefaultAsync() ?? new() { UserID = userId };
        }

        public Task<DailyAccountStats> GetUserDailyStatsAsync(string userId, CurrentServerRefresh<IDailyServerRefresh> refresh)
        {
            return Task.FromResult(new DailyAccountStats());
        }
    }
}
