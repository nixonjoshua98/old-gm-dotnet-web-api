using GMServer.Mongo.Models;
using GMServer.Mongo.Repositories;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class CurrenciesService
    {
        private readonly ICurrenciesRepository _currencies;

        public CurrenciesService(ICurrenciesRepository currencies)
        {
            _currencies = currencies;
        }

        public async Task<UserCurrencies> GetUserCurrenciesAsync(string userId)
        {
            return await _currencies.FindOneAsync(doc => doc.UserID == userId) ?? new() { UserID = userId };
        }

        public async Task<UserCurrencies> IncrementAsync(string userId, UserCurrencies incr)
        {
            return await _currencies.FindOneAndUpdateAsync(doc => doc.UserID == userId, upd =>
            {
                return upd
                    .Inc(x => x.Diamonds, incr.Diamonds)
                    .Inc(x => x.PrestigePoints, incr.PrestigePoints)
                    .Inc(x => x.BountyPoints, incr.BountyPoints)
                    .Inc(x => x.ArmouryPoints, incr.ArmouryPoints);

            }, isUpsert: true);
        }

        public async Task UpdateUserAsync(string userId, Func<UpdateDefinitionBuilder<UserCurrencies>, UpdateDefinition<UserCurrencies>> update)
        {
            await _currencies.UpdateOneAsync(doc => doc.UserID == userId, update, upsert: true);
        }
    }
}