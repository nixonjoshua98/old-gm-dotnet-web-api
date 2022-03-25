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
        private readonly IMongoCollection<UserDailyQuest> _dailyQuests;
        private readonly IMongoCollection<UserMercQuest> _mercQuests;

        public QuestsService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _dailyQuests = mongo.GetCollection<UserDailyQuest>("UserDailyQuests");
            _mercQuests = mongo.GetCollection<UserMercQuest>("UserMercQuests");
        }

        public async Task<List<int>> GetCompletedDailyQuestsAsync(string userId, CurrentServerRefresh refresh)
        {
            var ls = await _dailyQuests.Find(x => x.UserID == userId && x.CompletedTime > refresh.Previous && x.CompletedTime < refresh.Next).ToListAsync();

            return ls.Select(x => x.QuestID).ToList();
        }

        public async Task<List<int>> GetCompletedMercQuestsAsync(string userId)
        {
            return (await _mercQuests.Find(x => x.UserID == userId).ToListAsync()).Select(x => x.QuestID).ToList();
        }

        public QuestsDataFile GetDataFile()
        {
            return _cache.Load<QuestsDataFile>(DataFiles.Quests);
        }
    }
}
