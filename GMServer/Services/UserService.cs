using GMServer.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase mongo)
        {
            _users = mongo.GetCollection<User>("Accounts");
        }

        public async Task InsertUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<User> GetUserByDeviceIDAsync(string deviceID)
        {
            return await _users.Find(user => user.DeviceID == deviceID).FirstOrDefaultAsync();
        }
    }
}