using GMServer.Models.MongoModels;
using GMServer.UserModels.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class CurrenciesService
    {
        private readonly IMongoCollection<UserCurrencies> _currencies;

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
            return await UpdateCurrenciesAsync(userId, MongoDynamicUpdate.CreateDictionary(incr));
        }

        async Task<UserCurrencies> UpdateCurrenciesAsync(string userId, Dictionary<string, object> incr)
        {
            var filter = Builders<UserCurrencies>.Filter.Eq(x => x.UserID, userId);
            var update = new List<UpdateDefinition<UserCurrencies>>();

            foreach (var pair in incr)
            {
                update.Add(Builders<UserCurrencies>.Update
                    .Inc(pair.Key, pair.Value));
            }

            return await _currencies.FindOneAndUpdateAsync(filter, Builders<UserCurrencies>.Update.Combine(update), new() { IsUpsert = true, ReturnDocument = ReturnDocument.After });
        }
    }
}