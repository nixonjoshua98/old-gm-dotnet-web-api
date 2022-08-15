using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IPrestigeRepository : BaseClasses.IMongoRepository<UserPrestige>
    {

    }

    public class PrestigeRepository : BaseClasses.MongoRepository<UserPrestige>, IPrestigeRepository
    {
        public PrestigeRepository(IMongoClient mongo) : base(mongo, "GMServer.UserPrestiges")
        {

        }
    }
}
