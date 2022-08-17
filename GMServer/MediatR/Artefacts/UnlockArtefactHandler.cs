﻿using MediatR;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Types;
using SRC.DataFiles.Cache;
using SRC.Mongo.Models;
using SRC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.Artefacts
{
    public record UnlockArtefactCommand(string UserID) : IRequest<Result<UnlockArtefactResponse>>;

    public record UnlockArtefactResponse(UserArtefact Artefact, double UnlockCost);

    public class UnlockArtefactHandler : IRequestHandler<UnlockArtefactCommand, Result<UnlockArtefactResponse>>
    {
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;

        public UnlockArtefactHandler(ArtefactsService artefacts, CurrenciesService currencies, IDataFileCache dataFiles)
        {
            _currencies = currencies;
            _artefacts = artefacts;
            _dataFiles = dataFiles;
        }

        public async Task<Result<UnlockArtefactResponse>> Handle(UnlockArtefactCommand request, CancellationToken cancellationToken)
        {
            var datafile = _dataFiles.Artefacts;

            var unlockedArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            if (unlockedArtefacts.Count >= datafile.Count)
                return new ServerError("All artefacts already unlocked", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            double unlockCost = CalculateUnlockCost(unlockedArtefacts);

            if (unlockCost > userCurrencies.PrestigePoints)
                return new ServerError("Cannot afford artefact unlock", 400);

            UserArtefact newArtefact = GetNewArtefact(request.UserID, unlockedArtefacts, datafile);

            await _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = -unlockCost });

            await _artefacts.InsertArtefactAsync(newArtefact);

            return new UnlockArtefactResponse(newArtefact, unlockCost);
        }

        private UserArtefact GetNewArtefact(string userId, List<UserArtefact> userArtefacts, List<Artefact> artefacts)
        {
            IEnumerable<int> userArtefactIds = userArtefacts.Select(x => x.ArtefactID);

            var availArtefacts = artefacts.Where(x => !userArtefactIds.Contains(x.ID)).ToList();

            Artefact newArtefact = availArtefacts[Random.Shared.Next(availArtefacts.Count)];

            return new UserArtefact() { UserID = userId, ArtefactID = newArtefact.ID, Level = 1 };
        }

        private double CalculateUnlockCost(List<UserArtefact> userArtefacts)
        {
            return Math.Max(1, userArtefacts.Count) * Math.Pow(1.35, userArtefacts.Count);
        }
    }
}
