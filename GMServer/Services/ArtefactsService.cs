using GMServer.UserModels.DataFileModels;
using GMServer.UserModels.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class ArtefactsService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserArtefact> _artefacts;

        public ArtefactsService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _artefacts = mongo.GetCollection<UserArtefact>("Artefacts");
        }

        public async Task<List<UserArtefact>> GetUserArtefactsAsync(string userId)
        {
            return await _artefacts.Find(x => x.UserID == userId).ToListAsync();
        }

        public List<Artefact> GetDataFile()
        {
            return _cache.Load<List<Artefact>>(DataFiles.Artefacts);
        }
    }
}
