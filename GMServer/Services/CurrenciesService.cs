using MongoDB.Driver;
using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System;
using System.Threading.Tasks;

namespace SRC.Services
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
                    .Inc(x => x.Gemstones, incr.Gemstones)
                    .Inc(x => x.PrestigePoints, incr.PrestigePoints)
                    .Inc(x => x.BountyPoints, incr.BountyPoints)
                    .Inc(x => x.ArmouryPoints, incr.ArmouryPoints);

            }, isUpsert: true);
        }

        public async Task UpdateUserAsync(string userId, Func<UpdateDefinitionBuilder<UserCurrencies>, UpdateDefinition<UserCurrencies>> update)
        {
            await _currencies.UpdateOneAsync(doc => doc.UserID == userId, update, upsert: true);
        }

        public async Task<UserCurrencies> UpdateUserAsync(IClientSessionHandle session, string userId, Func<UpdateDefinitionBuilder<UserCurrencies>, UpdateDefinition<UserCurrencies>> update)
        {
            return await _currencies.FindOneAndUpdateAsync(session, doc => doc.UserID == userId, update(_currencies.Update), upsert: true);
        }
    }
}