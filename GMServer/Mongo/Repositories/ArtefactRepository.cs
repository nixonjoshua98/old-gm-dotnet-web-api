using GMServer.Mongo.Models;
using MongoDB.Driver;

namespace GMServer.Mongo.Repositories
{
    public interface IArtefactRepository : BaseClasses.IMongoRepository<UserArtefact>
    {

    }

    public class ArtefactRepository : BaseClasses.MongoRepository<UserArtefact>, IArtefactRepository
    {
        public ArtefactRepository(IMongoClient mongo) : base(mongo, "GMServer.ArtefactsV2")
        {

        }
    }
}
