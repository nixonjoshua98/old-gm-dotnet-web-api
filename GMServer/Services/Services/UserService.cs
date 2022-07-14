using GMServer.Models;
using GMServer.Repositories;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GMServer.Services
{
    public interface IUserService
    {
        User GetUserByAccessToken(string token);
        Task<User> GetUserByDeviceAsync(string deviceID);
        Task InsertUserAsync(User user);
        Task UpdateAccessTokenAsync(User user, string token);
    }

    public class UserService : AbstractMongoRepository<User>, IUserService
    {
        public UserService(IMongoDatabase mongo) : base(mongo, "Users")
        {

        }

        public async Task UpdateAccessTokenAsync(User user, string token)
        {
            await UpdateOneAsync(x => x.ID == user.ID, () => Update.Set(x => x.AccessToken, token));
        }

        public User GetUserByAccessToken(string token)
        {
            return FindOneAsync(x => x.AccessToken == token).Result;
        }

        public async Task<User> GetUserByDeviceAsync(string deviceID)
        {
            return await FindOneAsync(x => x.DeviceID == deviceID);
        }

        public async Task InsertUserAsync(User user)
        {
            await InsertOneAsync(user);
        }
    }
}