using GMServer.Exceptions;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.ArtefactHandler
{
    public class UnlockArtefactRequest : IRequest<UnlockArtefactResponse>
    {
        public string UserID;
    }

    public record UnlockArtefactResponse(UserArtefact Artefact, double UnlockCost);

    public class UnlockArtefactHandler : IRequestHandler<UnlockArtefactRequest, UnlockArtefactResponse>
    {
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;

        public UnlockArtefactHandler(ArtefactsService artefacts, CurrenciesService currencies)
        {
            _currencies = currencies;
            _artefacts = artefacts;
        }

        public async Task<UnlockArtefactResponse> Handle(UnlockArtefactRequest request, CancellationToken cancellationToken)
        {
            var datafile = _artefacts.GetDataFile();

            var unlockedArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            if (unlockedArtefacts.Count >= datafile.Count)
                throw new ServerException("All artefacts unlocked", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            double unlockCost = CalculateUnlockCost(unlockedArtefacts);

            if (unlockCost > userCurrencies.PrestigePoints)
                throw new ServerException("Cannot afford artefact unlock", 400);

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
