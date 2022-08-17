using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
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
