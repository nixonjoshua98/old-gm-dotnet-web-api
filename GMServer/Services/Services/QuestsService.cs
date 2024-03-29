﻿using GMServer.Cache;
using GMServer.Common;
using GMServer.Context;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class QuestsService
    {
        private readonly IDataFileCache _cache;

        private readonly IMongoCollection<UserDailyQuest> _dailies;
        private readonly IMongoCollection<UserMercQuest> _mercs;

        public QuestsService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;

            _dailies = mongo.GetCollection<UserDailyQuest>("UserQuestsProgress");
            _mercs = mongo.GetCollection<UserMercQuest>("UserQuestsProgress");
        }

        public async Task<List<int>> GetCompletedDailyQuestsAsync(string userId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            var ls = await _dailies.Find(_DailyQuestFilter(userId, refresh)).ToListAsync();

            return ls.Select(x => x.QuestID).ToList();
        }

        public async Task InsertQuestProgress(UserDailyQuest quest)
        {
            await _dailies.InsertOneAsync(quest);
        }

        public async Task InsertQuestProgress(UserMercQuest quest)
        {
            await _mercs.InsertOneAsync(quest);
        }

        public async Task<UserMercQuest> GetMercQuestProgressAsync(string userId, int questId)
        {
            return await _mercs.Find(_MercQuestFilter(userId, questId)).FirstOrDefaultAsync();
        }

        public async Task<UserDailyQuest> GetDailyQuestProgressAsync(string userId, int questId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            return await _dailies.Find(_DailyQuestFilter(userId, questId, refresh)).FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetCompletedMercQuestsAsync(string userId)
        {
            return (await _mercs.Find(x => x.UserID == userId).ToListAsync()).Select(x => x.QuestID).ToList();
        }

        /* Data File */

        public QuestsDataFile GetDataFile()
        {
            return _cache.Load<QuestsDataFile>(DataFiles.Quests);
        }

        /* Filters */

        FilterDefinition<UserMercQuest> _MercQuestFilter(string userId, int questId)
        {
            return Builders<UserMercQuest>.Filter.Where(x => 
                x.UserID == userId && 
                x.QuestID == questId && 
                x.QuestType == QuestType.Merc
                );
        }

        FilterDefinition<UserDailyQuest> _DailyQuestFilter(string userId, int questId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            return Builders<UserDailyQuest>.Filter.Where(x => 
                x.UserID == userId && 
                x.QuestID == questId && 
                x.CompletedTime >= refresh.Previous && 
                x.CompletedTime < refresh.Next &&
                x.QuestType == QuestType.Daily
                );
        }

        FilterDefinition<UserDailyQuest> _DailyQuestFilter(string userId, CurrentServerRefresh<IDailyRefresh> refresh)
        {
            return Builders<UserDailyQuest>.Filter.Where(x =>
                x.UserID == userId &&
                x.CompletedTime >= refresh.Previous &&
                x.CompletedTime < refresh.Next &&
                x.QuestType == QuestType.Daily
                );
        }
    }
}
