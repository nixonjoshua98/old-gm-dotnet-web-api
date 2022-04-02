using GMServer.Exceptions;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public class UpgradeBountyRequest : IRequest<UpgradeBountyResponse>
    {
        public string UserID;
        public int BountyID;
    }

    public record UpgradeBountyResponse(UserBounty Bounty);

    public class UpgradeBountyHandler : IRequestHandler<UpgradeBountyRequest, UpgradeBountyResponse>
    {
        private readonly BountiesService _bounties;

        public UpgradeBountyHandler(BountiesService bounties)
        {
            _bounties = bounties;
        }

        public async Task<UpgradeBountyResponse> Handle(UpgradeBountyRequest request, CancellationToken cancellationToken)
        {
            BountiesDataFile datafile = _bounties.GetDataFile();

            UserBounty userBounty = await _bounties.GetUserBountyAsync(request.UserID, request.BountyID);
            Bounty bounty = datafile.Bounties.FirstOrDefault(x => x.BountyID == request.BountyID);

            if (bounty is null || bounty is null)
                throw new ServerException("Bounty not found", 400);

            else if (!CanLevelUp(userBounty, bounty))
                throw new ServerException("Cannot level up bounty", 400);

            await _bounties.IncrementBountyLevel(request.UserID, request.BountyID, 1);

            userBounty.Level++; // Update the bounty (saves a database request)

            return new UpgradeBountyResponse(userBounty);
        }

        bool CanLevelUp(UserBounty userBounty, Bounty bounty)
        {
            var highestLevel = bounty.Levels.LastOrDefault(x => userBounty.NumDefeats >= x.NumDefeatsRequired);

            return highestLevel is null ? false : (bounty.Levels.IndexOf(highestLevel) + 1) > userBounty.Level;
        }
    }
}
