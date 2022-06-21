using GMServer.Cache;
using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.MongoModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class MercsService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserMerc> _mercs;

        public MercsService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _mercs = mongo.GetCollection<UserMerc>("UnlockedMercs");
        }

        public async Task UpdateMercs(string userId, List<MercUpdateModel> models)
        {
            var requests = new List<UpdateOneModel<UserMerc>>();

            foreach (var model in models)
            {
                (var filter, var update) = CreateQuery(userId, model);

                requests.Add(new(filter, update));
            }

            await _mercs.BulkWriteAsync(requests);
        }

        public async Task InsertMercAsync(UserMerc merc)
        {
            await _mercs.InsertOneAsync(merc);
        }

        public async Task<UserMerc> GetMerc(string userId, int mercId)
        {
            return await _mercs.Find(x => x.UserID == userId && x.MercID == mercId).FirstOrDefaultAsync();
        }

        public async Task<List<UserMerc>> GetUserMercsAsync(string userId)
        {
            return await _mercs.Find(m => m.UserID == userId).ToListAsync();
        }

        public MercsDataFile GetDataFile()
        {
            return _cache.Load<MercsDataFile>(DataFiles.Mercs);
        }

        private (FilterDefinition<UserMerc>, UpdateDefinition<UserMerc>) CreateQuery(string userId, MercUpdateModel model)
        {
            FilterDefinition<UserMerc> filter = Builders<UserMerc>.Filter.Where(x => x.UserID == userId && x.MercID == model.MercID);

            UpdateDefinition<UserMerc> update = Builders<UserMerc>.Update
                .Inc(x => x.ExpertiseLevel, model.Levels)
                .Inc(x => x.ExpertiseExp, model.ExpertiseExp)
                .Inc(x => x.UpgradePoints, model.UpgradePoints);

            return (filter, update);
        }
    }
}
