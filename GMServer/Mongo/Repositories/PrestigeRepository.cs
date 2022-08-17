using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
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
