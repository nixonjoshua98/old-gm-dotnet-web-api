using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
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
