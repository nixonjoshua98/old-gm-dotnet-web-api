using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Threading.Tasks;
using GMServer.Extensions;

namespace GMServer.Services
{
    public class CurrenciesService
    {
        private IMongoCollection<UserCurrencies> _currencies;

        public CurrenciesService(IMongoDatabase mongo)
        {
            _currencies = mongo.GetCollection<UserCurrencies>("Currencies");
        }

        public async Task<UserCurrencies> GetUserCurrenciesAsync(string userId)
        {
            return await _currencies.Find(x => x.UserID == userId).FirstOrDefaultAsync() ?? new() { UserID = userId };
        }

        public async Task<UserCurrencies> IncrementAsync(string userId, UserCurrencies incr)
        {
            var filter = Builders<UserCurrencies>.Filter.Eq(x => x.UserID, userId);
            var update = Builders<UserCurrencies>.Update
                .Inc(x => x.Diamonds, incr.Diamonds)
                .Inc(x => x.PrestigePoints, incr.PrestigePoints)
                .Inc(x => x.BountyPoints, incr.BountyPoints)
                .Inc(x => x.ArmouryPoints, incr.ArmouryPoints);

            return await _currencies.FindOneAndUpdateAsync(filter, update, new() { IsUpsert = true, ReturnDocument = ReturnDocument.After });
        }
    }
}