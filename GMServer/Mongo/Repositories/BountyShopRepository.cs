using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
{
    public interface IBountyShopRepository : BaseClasses.IMongoRepository<BountyShopModel>
    {

    }

    public class BountyShopRepository : BaseClasses.MongoRepository<BountyShopModel>, IBountyShopRepository
    {
        public BountyShopRepository(IMongoClient mongo) : base(mongo, "GMServer.BountyShop")
        {

        }
    }
}
