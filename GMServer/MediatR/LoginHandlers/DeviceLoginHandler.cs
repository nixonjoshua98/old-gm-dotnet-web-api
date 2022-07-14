using GMServer.Models;
using GMServer.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.Login
{
    public class DeviceLoginRequest : IRequest<DeviceLoginResponse>
    {
        public string DeviceID;
    }

    public class DeviceLoginResponse : AbstractLoginResponse
    {
        public string UserID;
        public string Token;

        public DeviceLoginResponse(string userID, string token)
        {
            UserID = userID;
            Token = token;
        }
    }

    public class DeviceLoginHandler : IRequestHandler<DeviceLoginRequest, DeviceLoginResponse>
    {
        private readonly AuthenticationService _auth;
        private readonly IUserService _users;

        public DeviceLoginHandler(AuthenticationService auth, IUserService users)
        {
            _auth = auth;
            _users = users;
        }

        public async Task<DeviceLoginResponse> Handle(DeviceLoginRequest request, CancellationToken cancellationToken)
        {
            User user = await _users.GetUserByDeviceAsync(request.DeviceID);

            if (user is null)
            {
                user = new User() { DeviceID = request.DeviceID };

                await _users.InsertUserAsync(user);
            }

            string newAccessToken = _auth.GenerateAccessToken();

            await _users.UpdateAccessTokenAsync(user, newAccessToken);

            return new DeviceLoginResponse(user.ID, newAccessToken);
        }
    }
}
