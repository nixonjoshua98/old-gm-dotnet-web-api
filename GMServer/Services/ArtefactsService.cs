using MongoDB.Driver;
using SRC.Models.RequestModels;
using SRC.Mongo.Models;
using SRC.Mongo.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SRC.Services
{
    public class ArtefactsService
    {
        private readonly IArtefactRepository _artefacts;

        public ArtefactsService(IArtefactRepository artefacts)
        {
            _artefacts = artefacts;
        }

        public async Task<List<UserArtefact>> GetUserArtefactsAsync(string userId)
        {
            return await _artefacts.FindAsync(x => x.UserID == userId);
        }

        public async Task InsertArtefactAsync(UserArtefact artefact)
        {
            await _artefacts.InsertOneAsync(artefact);
        }

        public async Task BulkUpgradeArtefacts(string userId, List<UserArtefactUpgrade> upgrades)
        {
            var requests = new List<WriteModel<UserArtefact>>();

            foreach (UserArtefactUpgrade upgrade in upgrades)
            {
                var filter = _artefacts.Filter.Where(x => x.UserID == userId && x.ArtefactID == upgrade.ArtefactID);

                var query = new UpdateOneModel<UserArtefact>(filter, _artefacts.Update.Inc(x => x.Level, upgrade.Levels));

                requests.Add(query);
            }

            await _artefacts.BulkWriteAsync(requests);
        }
    }
}
