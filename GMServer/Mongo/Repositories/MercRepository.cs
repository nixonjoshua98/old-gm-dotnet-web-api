using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IMercRepository : BaseClasses.IMongoRepository<UserMerc>
    {

    }

    public class MercRepository : BaseClasses.MongoRepository<UserMerc>, IMercRepository
    {
        public MercRepository(IMongoClient mongo) : base(mongo, "GMServer.UnlockedMercs")
        {

        }
    }
}
