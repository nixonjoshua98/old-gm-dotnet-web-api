using GMServer.Authentication;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.Login
{
    public record DeviceLoginCommand(string DeviceID) : IRequest<DeviceLoginResponse>;

    public record DeviceLoginResponse(string UserID, string Token);

    public class DeviceLoginHandler : IRequestHandler<DeviceLoginCommand, DeviceLoginResponse>
    {
        private readonly IUserService _users;

        public DeviceLoginHandler(IUserService users)
        {
            _users = users;
        }

        public async Task<DeviceLoginResponse> Handle(DeviceLoginCommand request, CancellationToken cancellationToken)
        {
            User user = await _users.GetUserByDeviceAsync(request.DeviceID);

            if (user is null)
            {
                user = new User() { DeviceID = request.DeviceID };

                await _users.InsertUserAsync(user);
            }

            string newAccessToken = AuthUtility.GenerateAccessToken();

            await _users.UpdateAccessTokenAsync(user, newAccessToken);

            return new DeviceLoginResponse(user.ID, newAccessToken);
        }
    }
}
