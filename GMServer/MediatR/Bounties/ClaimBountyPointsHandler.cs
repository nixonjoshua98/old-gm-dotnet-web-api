using MediatR;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Types;
using SRC.DataFiles.Cache;
using SRC.Mongo;
using SRC.Mongo.Models;
using SRC.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.BountyHandlers
{
    public record ClaimBountyPointRequest(string UserID) : IRequest<Result<ClaimBountyPointsResponse>>
    {

    }

    public record ClaimBountyPointsResponse(long PointsClaimed,
                                            UserCurrencies Currencies);

    public class ClaimBountyPointsHandler : IRequestHandler<ClaimBountyPointRequest, Result<ClaimBountyPointsResponse>>
    {
        private readonly IBountiesService _bounties;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;
        private readonly IMongoSessionFactory _mongoSession;

        public ClaimBountyPointsHandler(CurrenciesService currencies, IBountiesService bounties, IDataFileCache dataFiles, IMongoSessionFactory mongoSession)
        {
            _bounties = bounties;
            _currencies = currencies;
            _dataFiles = dataFiles;
            _mongoSession = mongoSession;
        }

        public async Task<Result<ClaimBountyPointsResponse>> Handle(ClaimBountyPointRequest request, CancellationToken cancellationToken)
        {
            DateTime dt = DateTime.UtcNow;

            var userBounties = await _bounties.GetUserBountiesAsync(request.UserID);

            long points = CalculateClaimPoints(dt, userBounties);

            // Update the claim time and increment the points earned
            UserCurrencies updatedCurrencies = await _mongoSession.RunInTransaction(async (session, cToken) =>
            {
                await _bounties.UpdateUserAsync(session, request.UserID, upd => upd.Set(doc => doc.LastClaimTime, dt));

                return await _currencies.UpdateUserAsync(session, request.UserID, upd => upd.Inc(doc => doc.BountyPoints, points));
            });

            return new ClaimBountyPointsResponse(points, updatedCurrencies);
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
