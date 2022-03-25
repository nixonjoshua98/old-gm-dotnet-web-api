using GMServer.Services;
using GMServer.UserModels.DataFileModels;
using GMServer.UserModels.UserModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyHandlers
{
    public class ClaimBountyPointRequest : IRequest<ClaimBountyPoinResponse>
    {
        public string UserID;
        public DateTime DateTime;
    }

    public record ClaimBountyPoinResponse(DateTime ClaimTime, long PointsClaimed, UserCurrencies Currencies);

    public class ClaimBountyPointsHandler : IRequestHandler<ClaimBountyPointRequest, ClaimBountyPoinResponse>
    {
        private readonly BountiesService _bounties;
        private readonly CurrenciesService _currencies;

        public ClaimBountyPointsHandler(CurrenciesService currencies, BountiesService bounties)
        {
            _bounties = bounties;
            _currencies = currencies;
        }

        public async Task<ClaimBountyPoinResponse> Handle(ClaimBountyPointRequest request, CancellationToken cancellationToken)
        {
            var userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            long points = CalculateClaimPoints(request.DateTime, userBounties);

            await _bounties.SetClaimTimeAsync(request.UserID, request.DateTime);

            UserCurrencies updatedCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = points });

            return new(request.DateTime, points, updatedCurrencies);
        }

        long CalculateClaimPoints(DateTime now, UserBounties bounties)
        {
            var datafile = _bounties.GetDataFile();

            List<UserBounty> activeBounties = bounties.UnlockedBounties.Where(x => bounties.ActiveBounties.Contains(x.BountyID)).ToList();

            double hoursSinceClaim = Math.Clamp((now - bounties.LastClaimTime).TotalHours, 0, datafile.MaxUnclaimedHours);

            double cumPoints = 0;

            foreach (var userBounty in activeBounties)
            {
                Bounty bounty = datafile.Bounties.FirstOrDefault(x => x.BountyID == userBounty.BountyID);

                if (bounty is not null)
                {
                    cumPoints += hoursSinceClaim * bounty.HourlyIncome;
                }
            }

            return Convert.ToInt64(cumPoints);
        }
    }
}
