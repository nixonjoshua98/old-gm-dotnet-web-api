using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
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
