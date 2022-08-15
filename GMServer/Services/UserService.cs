using GMServer.Mongo.Models;
using GMServer.Mongo.Repositories;
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

    public class UserService : IUserService
    {
        private readonly IUserRepository _users;

        public UserService(IUserRepository users)
        {
            _users = users;
        }

        public async Task UpdateAccessTokenAsync(User user, string token)
        {
            await _users.UpdateOneAsync(doc => doc.ID == user.ID, update => update.Set(x => x.AccessToken, token));
        }

        public User GetUserByAccessToken(string token)
        {
            return _users.FindOneAsync(x => x.AccessToken == token).Result;
        }

        public async Task<User> GetUserByDeviceAsync(string deviceID)
        {
            return await _users.FindOneAsync(x => x.DeviceID == deviceID);
        }

        public async Task InsertUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }
    }
}