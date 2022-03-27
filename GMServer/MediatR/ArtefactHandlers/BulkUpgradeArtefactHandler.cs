using GMServer.Common;
using GMServer.Exceptions;
using GMServer.Models.DataFileModels;
using GMServer.Models.RequestModels;
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
    public class BulkUpgradeArtefactRequest : IRequest<BulkUpgradeArtefactResponse>
    {
        public string UserID;
        public List<UserArtefactUpgrade> Artefacts;
    }

    public record BulkUpgradeArtefactResponse(List<UserArtefact> Artefacts, double UpgradeCost, double RemainingPrestigePoints);

    public class BulkUpgradeArtefactHandler : IRequestHandler<BulkUpgradeArtefactRequest, BulkUpgradeArtefactResponse>
    {
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;

        public BulkUpgradeArtefactHandler(ArtefactsService artefacts, CurrenciesService currencies)
        {
            _currencies = currencies;
            _artefacts = artefacts;
        }

        public async Task<BulkUpgradeArtefactResponse> Handle(BulkUpgradeArtefactRequest request, CancellationToken cancellationToken)
        {
            var userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            var datafile = _artefacts.GetDataFile();

            if (request.Artefacts.Count == 0)
                throw new ServerException("No artefacts provided", 400);

            else if (!AllUniqueArtefacts(request.Artefacts))
                throw new ServerException("Duplicate artefacts found", 400);

            else if (!UserOwnsAllArtefacts(userArtefacts, request.Artefacts))
                throw new ServerException("Invalid artefact found", 400);

            else if (!UpgradeLevelsValid(userArtefacts, datafile, request.Artefacts))
                throw new ServerException("Some artefact upgrade requests are not valid", 400);

            // Calculate total upgrade cost for all upgrades
            double totalUpgradeCost = CalculateTotalUpgradeCost(userArtefacts, datafile, request.Artefacts);

            UserCurrencies currencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (totalUpgradeCost <= 0 || totalUpgradeCost > currencies.PrestigePoints)
                throw new ServerException("Cannot afford upgrade", 400);

            // Decrement the upgrade cost and get the new values
            var userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = -totalUpgradeCost });

            // Perform the update on the database
            await _artefacts.BulkUpgradeArtefacts(request.UserID, request.Artefacts);

            // Re-fetch the updated artefacts
            userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            return new BulkUpgradeArtefactResponse(userArtefacts, totalUpgradeCost, userCurrencies.PrestigePoints);
        }

        private bool UpgradeLevelsValid(List<UserArtefact> unlockedArtefacts, List<Artefact> artefactsDatafile, List<UserArtefactUpgrade> artefactUpgrades)
        {
            foreach (var artUpgrade in artefactUpgrades)
            {
                Artefact artefact = artefactsDatafile.FirstOrDefault(x => x.ID == artUpgrade.ArtefactID);
                UserArtefact userArtefact = unlockedArtefacts.FirstOrDefault(x => x.ArtefactID == artUpgrade.ArtefactID);

                if (artUpgrade.Levels <= 0 || userArtefact.Level + artUpgrade.Levels > artefact.MaxLevel)
                {
                    return false;
                }
            }

            return true;
        }

        private bool UserOwnsAllArtefacts(List<UserArtefact> unlockedArtefacts, List<UserArtefactUpgrade> artefacts)
        {
            IEnumerable<int> unlockedArtefactsId = unlockedArtefacts.Select(x => x.ArtefactID);

            return artefacts.All(art => unlockedArtefactsId.Contains(art.ArtefactID));
        }

        private bool AllUniqueArtefacts(List<UserArtefactUpgrade> artefacts)
        {
            return artefacts.Select(x => x.ArtefactID).ToHashSet().Count() == artefacts.Count;
        }

        private double CalculateTotalUpgradeCost(List<UserArtefact> unlockedArtefacts, List<Artefact> artefactsDatafile, List<UserArtefactUpgrade> artefactUpgrades)
        {
            double totalCost = 0;

            foreach (var artUpgrade in artefactUpgrades)
            {
                Artefact artefact = artefactsDatafile.First(x => x.ID == artUpgrade.ArtefactID);
                UserArtefact userArtefact = unlockedArtefacts.First(x => x.ArtefactID == artUpgrade.ArtefactID);

                totalCost += CalculateUpgradeCost(userArtefact, artefact, artUpgrade.Levels);
            }

            return totalCost;
        }

        private double CalculateUpgradeCost(UserArtefact userArtefact, Artefact artefact, int levels)
        {
            return artefact.CostCoeff * GameFormulas.SumNonIntegerPowerSeq(userArtefact.Level, levels, artefact.CostExpo);
        }
    }
}