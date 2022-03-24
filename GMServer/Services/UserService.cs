using GMServer.Models;
using MongoDB.Driver;
using System.Linq;
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

        public User GetUser(string id)
        {
            return _users.Find(user => user.ID == id).FirstOrDefault();
        }

        public async Task<User> GetUserByDeviceIDAsync(string deviceID)
        {
            return await _users.Find(user => user.DeviceID == deviceID).FirstOrDefaultAsync();
        }
    }
}