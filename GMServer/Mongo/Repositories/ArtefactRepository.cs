using MongoDB.Driver;
using SRC.Mongo.Models;

namespace SRC.Mongo.Repositories
{
    public interface IArtefactRepository : BaseClasses.IMongoRepository<UserArtefact>
    {

    }

    public class ArtefactRepository : BaseClasses.MongoRepository<UserArtefact>, IArtefactRepository
    {
        public ArtefactRepository(IMongoClient mongo) : base(mongo, "GMServer.Artefacts")
        {

        }
    }
}
