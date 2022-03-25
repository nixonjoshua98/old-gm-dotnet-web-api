using GMServer.UserModels.DataFileModels;
using GMServer.UserModels.UserModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class ArmouryService
    {
        private readonly IDataFileCache _cache;
        private readonly IMongoCollection<UserArmouryItem> _armoury;

        public ArmouryService(IDataFileCache cache, IMongoDatabase mongo)
        {
            _cache = cache;
            _armoury = mongo.GetCollection<UserArmouryItem>("ArmouryItems");
        }

        public async Task<List<UserArmouryItem>> GetUserArmouryItemsAsync(string userId)
        {
            return await _armoury.Find(x => x.UserID == userId).ToListAsync();
        }

        public List<ArmouryItem> GetDataFile()
        {
            return _cache.Load<List<ArmouryItem>>(DataFiles.Armoury);
        }
    }
}
