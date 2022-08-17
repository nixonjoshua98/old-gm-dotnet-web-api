﻿using MediatR;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Types;
using SRC.DataFiles.Cache;
using SRC.Mongo.Models;
using SRC.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.BountyHandlers
{
    public record UpgradeBountyCommand(string UserID, int BountyID) : IRequest<Result<UpgradeBountyResponse>>;

    public record UpgradeBountyResponse(UserBounty Bounty);

    public class UpgradeBountyHandler : IRequestHandler<UpgradeBountyCommand, Result<UpgradeBountyResponse>>
    {
        private readonly IBountiesService _bounties;
        private readonly IDataFileCache _dataFiles;

        public UpgradeBountyHandler(IBountiesService bounties, IDataFileCache dataFiles)
        {
            _bounties = bounties;
            _dataFiles = dataFiles;
        }

        public async Task<Result<UpgradeBountyResponse>> Handle(UpgradeBountyCommand request, CancellationToken cancellationToken)
        {
            BountiesDataFile datafile = _dataFiles.Bounties;

            UserBounty userBounty = await _bounties.GetUserBountyAsync(request.UserID, request.BountyID);
            Bounty bounty = datafile.Bounties.FirstOrDefault(x => x.BountyID == request.BountyID);

            // Perform some request validation
            if (!ValidateRequest(userBounty, bounty, out var error))
                return error;

            await _bounties.IncrementBountyLevelAsync(request.UserID, request.BountyID, 1);

            userBounty.Level++; // Update the bounty (saves a database request)

            return new UpgradeBountyResponse(userBounty);
        }

        private bool ValidateRequest(UserBounty ownedBounty, Bounty gameBounty, out ServerError error)
        {
            error = default;

            if (ownedBounty is null || gameBounty is null)
                error = new("Bounty not found", 400);

            else if (!CanLevelUp(ownedBounty, gameBounty))
                error = new("Cannot level up bounty", 400);

            return error == default;
        }

        private bool CanLevelUp(UserBounty userBounty, Bounty bounty)
        {
            var highestLevel = bounty.Levels.LastOrDefault(x => userBounty.CurrentKillCount >= x.KillsRequired);

            return highestLevel is not null && highestLevel.Level > userBounty.Level;
        }
    }
}
