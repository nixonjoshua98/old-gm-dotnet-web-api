using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public class ToggleActiveBountyRequest : IRequest<ToggleActiveBountyResponse>
    {
        public string UserID;
        public int BountyID;
        public bool IsActive;
    }

    public class ToggleActiveBountyResponse : AbstractResponseWithError
    {
        public ToggleActiveBountyResponse()
        {
        }

        public ToggleActiveBountyResponse(string message, int code) : base(message, code)
        {
        }
    }

    public class ToggleActiveBountyHandler : IRequestHandler<ToggleActiveBountyRequest, ToggleActiveBountyResponse>
    {
        private readonly IBountiesService _bounties;

        public ToggleActiveBountyHandler(IBountiesService bounties)
        {
            _bounties = bounties;
        }

        public async Task<ToggleActiveBountyResponse> Handle(ToggleActiveBountyRequest request, CancellationToken cancellationToken)
        {
            // Removing an active bounty requires no checking since it will simply not remove from the document
            // if the bounty does not exist or is currently active
            if (!request.IsActive)
            {
                await _bounties.RemoveActiveBountyAsync(request.UserID, request.BountyID);

                return new();
            }

            UserBounties bounties = await _bounties.GetUserBountiesAsync(request.UserID);

            // Bounty is either already active or not unlocked yet - We should return an error
            if (bounties.IsBountyActive(request.BountyID) || !bounties.IsBountyUnlocked(request.BountyID))
                return new("Bounty is either already active or not unlocked yet", 400);

            await _bounties.AddActiveBountyAsync(request.UserID, request.BountyID);

            return new();
        }
    }
}
