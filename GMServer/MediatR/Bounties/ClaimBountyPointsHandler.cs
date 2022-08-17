using GMServer.Caching.DataFiles.Models;
using GMServer.Common.Types;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using SRC.DataFiles.Cache;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public record ClaimBountyPointRequest(string UserID) : IRequest<Result<ClaimBountyPointsResponse>>
    {

    }

    public class ClaimBountyPointsResponse
    {
        public DateTime ClaimTime;
        public long PointsClaimed;
        public UserCurrencies Currencies;
    }

    public class ClaimBountyPointsHandler : IRequestHandler<ClaimBountyPointRequest, Result<ClaimBountyPointsResponse>>
    {
        private readonly IBountiesService _bounties;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;

        public ClaimBountyPointsHandler(CurrenciesService currencies, IBountiesService bounties, IDataFileCache dataFiles)
        {
            _bounties = bounties;
            _currencies = currencies;
            _dataFiles = dataFiles;
        }

        public async Task<Result<ClaimBountyPointsResponse>> Handle(ClaimBountyPointRequest request, CancellationToken cancellationToken)
        {
            DateTime dt = DateTime.UtcNow;

            var userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            long points = CalculateClaimPoints(dt, userBounties);

            if (points <= 0) // User cannot claim a zero balance
            {
                return new ServerError("Cannot claim zero points", 400);
            }

            // Set the claim time
            await _bounties.UpdateUserAsync(request.UserID, upd => upd.Set(doc => doc.LastClaimTime, dt));

            UserCurrencies updatedCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = points });

            return new ClaimBountyPointsResponse() { ClaimTime = dt, PointsClaimed = points, Currencies = updatedCurrencies };
        }

        private long CalculateClaimPoints(DateTime now, UserBounties bounties)
        {
            var datafile = _dataFiles.Bounties;

            double hoursSinceClaim = Math.Clamp((now - bounties.LastClaimTime).TotalHours, 0, datafile.MaxUnclaimedHours);

            double total = 0;

            foreach (var userBounty in bounties.UnlockedBounties)
            {
                Bounty bounty = datafile.Bounties.First(x => x.BountyID == userBounty.BountyID);

                total += hoursSinceClaim * bounty.PointsPerHour;
            }

            return (long)Math.Floor(total);
        }
    }
}
