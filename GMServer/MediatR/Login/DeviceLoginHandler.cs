using GMServer.Common;
using GMServer.Exceptions;
using GMServer.Models;
using GMServer.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.Login
{
    public record class DeviceLoginRequest : IRequest<DeviceLoginResponse>
    {
        public string DeviceID { get; set; }   
    }

    public record DeviceLoginResponse(string Token);

    public class DeviceLoginHandler : IRequestHandler<DeviceLoginRequest, DeviceLoginResponse>
    {
        private readonly AuthenticationService _auth;
        private readonly UserService _users;

        public DeviceLoginHandler(AuthenticationService auth, UserService users)
        {
            _auth = auth;
            _users = users;
        }

        public async Task<DeviceLoginResponse> Handle(DeviceLoginRequest request, CancellationToken cancellationToken)
        {
            User user = await _users.GetUserByDeviceIDAsync(request.DeviceID);

            if (user is null)
            {
                throw new ServerException("Account not found", HTTPCodes.AccountNotFound);
            }

            await _auth.InvalidateUserSessionsAsync(user);

            string token = _auth.CreateToken(user);

            AuthenticatedSession session = new()
            {
                Token = token,
                UserID = user.ID,
                DeviceID = request.DeviceID,
                CreatedAt = DateTime.Now
            };

            await _auth.InsertSessionAsync(session);

            return new DeviceLoginResponse(session.Token);
        }
    }
}
