using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IUserRepository : BaseClasses.IMongoRepository<User>
    {

    }

    public class UserRepository : BaseClasses.MongoRepository<User>, IUserRepository
    {
        public UserRepository(IMongoClient mongo) : base(mongo, "GMServer.Users")
        {

        }
    }
}
