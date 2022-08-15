using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using SRC.DataFiles.Cache;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public record ToggleActiveBountyCommand(string UserID, int BountyID, bool IsActive) : IRequest<ToggleActiveBountyResponse>;

    public record ToggleActiveBountyResponse;

    public class ToggleActiveBountyHandler : IRequestHandler<ToggleActiveBountyCommand, ToggleActiveBountyResponse>
    {
        private readonly IBountiesService _bounties;
        private readonly IDataFileCache _dataFiles;
        public ToggleActiveBountyHandler(IBountiesService bounties, IDataFileCache dataFiles)
        {
            _bounties = bounties;
            _dataFiles = dataFiles;
        }

        public async Task<ToggleActiveBountyResponse> Handle(ToggleActiveBountyCommand request, CancellationToken cancellationToken)
        {
            // Removing an active bounty requires no checking since it will simply not remove from the document
            // if the bounty does not exist or is currently active
            if (!request.IsActive)
            {
                await _bounties.RemoveActiveBountyAsync(request.UserID, request.BountyID);

                return new();
            }

            var datafile = _dataFiles.Bounties;

            UserBounties bounties = await _bounties.GetUserBountiesAsync(request.UserID);

            if (bounties.ActiveBounties.Count >= datafile.MaxActiveBounties)
                throw new System.Exception("Maximum active bounties reached");

            // Bounty is either already active or not unlocked yet - We should return an error
            if (bounties.IsBountyActive(request.BountyID) || !bounties.IsBountyUnlocked(request.BountyID))
                throw new System.Exception("Bounty is either already active or not unlocked yet");

            await _bounties.AddActiveBountyAsync(request.UserID, request.BountyID);

            return new();
        }
    }
}
