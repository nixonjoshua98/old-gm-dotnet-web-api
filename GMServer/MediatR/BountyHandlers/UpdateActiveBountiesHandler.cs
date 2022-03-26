using GMServer.Exceptions;
using GMServer.Services;
using GMServer.UserModels.UserModels;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GMServer.MediatR.BountyHandlers
{
    public class UpdateActiveBountiesRequest : IRequest<UpdateActiveBountiesResponse>
    {
        public string UserID;
        public HashSet<int> BountyIds;
    }

    public record UpdateActiveBountiesResponse();

    public class UpdateActiveBountiesHandler : IRequestHandler<UpdateActiveBountiesRequest, UpdateActiveBountiesResponse>
    {
        private readonly BountiesService _bounties;

        public UpdateActiveBountiesHandler(BountiesService bounties)
        {
            _bounties = bounties;
        }

        public async Task<UpdateActiveBountiesResponse> Handle(UpdateActiveBountiesRequest request, CancellationToken cancellationToken)
        {
            var datafile = _bounties.GetDataFile();

            if (request.BountyIds.Count > datafile.MaxActiveBounties)
                throw new ServerException("Active bounties will exceed the maximum", 400);

            var userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            if (!IsBountiesUnlocked(userBounties, request.BountyIds))
                throw new ServerException("Bounties not all unlocked", 400);

            await _bounties.SetActiveBountiesAsync(request.UserID, request.BountyIds);

            return new UpdateActiveBountiesResponse();
        }

        bool IsBountiesUnlocked(UserBounties bounties, IEnumerable<int> bountyIds)
        {
            var unlockedBountyIds = bounties.UnlockedBounties.Select(x => x.BountyID).ToList();

            return bountyIds.All(id => unlockedBountyIds.Contains(id));
        }
    }
}
