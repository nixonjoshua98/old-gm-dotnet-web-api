using GMServer.UserModels.UserModels;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class CurrenciesService
    {
        private readonly IMongoCollection<UserCurrencies> _armoury;

        public CurrenciesService(IMongoDatabase mongo)
        {
            _armoury = mongo.GetCollection<UserCurrencies>("Currencies");
        }

        public async Task<UserCurrencies> GetUserCurrenciesAsync(string userId)
        {
            return await _armoury.Find(x => x.UserID == userId).FirstOrDefaultAsync() ?? new() { UserID = userId };
        }
    }
}