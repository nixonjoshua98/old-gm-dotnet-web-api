using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public class ClaimBountyPointRequest : IRequest<ClaimBountyPointsResponse>
    {
        public string UserID;
    }

    public class ClaimBountyPointsResponse : AbstractResponseWithError
    {
        public DateTime ClaimTime;
        public long PointsClaimed;
        public UserCurrencies Currencies;

        public ClaimBountyPointsResponse() { }
        public ClaimBountyPointsResponse(string message, int code) : base(message, code) { }
        public static ClaimBountyPointsResponse AsError(string m, int code) => new(m, code);
    }

    public class ClaimBountyPointsHandler : IRequestHandler<ClaimBountyPointRequest, ClaimBountyPointsResponse>
    {
        private readonly IBountiesService _bounties;
        private readonly CurrenciesService _currencies;

        public ClaimBountyPointsHandler(CurrenciesService currencies, IBountiesService bounties)
        {
            _bounties = bounties;
            _currencies = currencies;
        }

        public async Task<ClaimBountyPointsResponse> Handle(ClaimBountyPointRequest request, CancellationToken cancellationToken)
        {
            DateTime dt = DateTime.UtcNow;

            var userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            long points = CalculateClaimPoints(dt, userBounties);

            if (points <= 0) // User cannot claim a zero balance
                return ClaimBountyPointsResponse.AsError("Cannot claim zero points", 400);

            await _bounties.SetClaimTimeAsync(request.UserID, dt);

            UserCurrencies updatedCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = points });

            return new() { ClaimTime = dt, PointsClaimed = points, Currencies = updatedCurrencies };
        }

        private long CalculateClaimPoints(DateTime now, UserBounties bounties)
        {
            var datafile = _bounties.GetDataFile();

            double hoursSinceClaim = Math.Clamp((now - bounties.LastClaimTime).TotalHours, 0, datafile.MaxUnclaimedHours);

            double total = 0;

            foreach (int bountyId in bounties.ActiveBounties)
            {
                Bounty bounty = datafile.Bounties.First(x => x.BountyID == bountyId);

                total += hoursSinceClaim * bounty.HourlyIncome;
            }

            return (long)Math.Floor(total);
        }
    }
}
