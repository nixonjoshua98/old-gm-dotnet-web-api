﻿using GMServer.Cache;
using GMServer.Common;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class UnitService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserMerc> _mercs;

        public UnitService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _mercs = mongo.GetCollection<UserMerc>("UnlockedMercs");
        }

        public async Task UpdateUnitXP(string userId, Dictionary<int, long> values)
        {
            var requests = new List<UpdateOneModel<UserMerc>>();

            foreach (var pair in values)
            {
                var filter = Builders<UserMerc>.Filter.Where(x => x.UserID == userId && x.MercID == pair.Key);

                var update = Builders<UserMerc>.Update
                    .Inc(x => x.CurrentXP, pair.Value)
                    .Inc(x => x.TotalXP, pair.Value);

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
    }
}