using GMServer.Models.RequestModels;
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

        public async Task InsertArtefactAsync(UserArtefact artefact)
        {
            await _artefacts.InsertOneAsync(artefact);
        }

        public async Task BulkUpgradeArtefacts(string userId, List<UserArtefactUpgrade> upgrades)
        {
            var requests = new List<UpdateOneModel<UserArtefact>>();

            foreach (UserArtefactUpgrade upgrade in upgrades)
            {
                var query = new UpdateOneModel<UserArtefact> (
                    Builders<UserArtefact>.Filter.Eq(x => x.UserID, userId) & Builders<UserArtefact>.Filter.Eq(x => x.ArtefactID, upgrade.ArtefactID),
                    Builders<UserArtefact>.Update.Inc(x => x.Level, upgrade.Levels)
                    );

                requests.Add(query);
            }

            await _artefacts.BulkWriteAsync(requests);
        }

        public List<Artefact> GetDataFile()
        {
            return _cache.Load<List<Artefact>>(DataFiles.Artefacts);
        }
    }
}
