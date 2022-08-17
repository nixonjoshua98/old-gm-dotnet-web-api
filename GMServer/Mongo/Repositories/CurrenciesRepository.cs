using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
{
    public interface ICurrenciesRepository : BaseClasses.IMongoRepository<UserCurrencies>
    {

    }

    public class CurrenciesRepository : BaseClasses.MongoRepository<UserCurrencies>, ICurrenciesRepository
    {
        public CurrenciesRepository(IMongoClient mongo) : base(mongo, "GMServer.Currencies")
        {

        }
    }
}
