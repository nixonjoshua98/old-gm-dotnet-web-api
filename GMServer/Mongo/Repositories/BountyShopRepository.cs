using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IBountyShopRepository : BaseClasses.IMongoRepository<UserBountyShopState>
    {

    }

    public class BountyShopRepository : BaseClasses.MongoRepository<UserBountyShopState>, IBountyShopRepository
    {
        public BountyShopRepository(IMongoClient mongo) : base(mongo, "GMServer.BountyShopPurchases")
        {

        }
    }
}
