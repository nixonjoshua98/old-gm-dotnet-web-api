using GMServer.Common;
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

    public class BulkUpgradeArtefactResponse : AbstractResponseWithError
    {
        public List<UserArtefact> Artefacts;
        public double UpgradeCost;
        public double RemainingPrestigePoints;

        public BulkUpgradeArtefactResponse(List<UserArtefact> artefacts, double upgradeCost, double remainingPrestigePoints)
        {
            Artefacts = artefacts;
            UpgradeCost = upgradeCost;
            RemainingPrestigePoints = remainingPrestigePoints;
        }

        public BulkUpgradeArtefactResponse(string message, int code) : base(message, code)
        {

        }
    }

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
                return new("No artefacts provided", 400);

            else if (!AllUniqueArtefacts(request.Artefacts))
                return new("Duplicate artefacts found", 400);

            else if (!UserOwnsAllArtefacts(userArtefacts, request.Artefacts))
                return new("Invalid artefact found", 400);

            else if (!UpgradeLevelsValid(userArtefacts, datafile, request.Artefacts))
                return new("Some artefact upgrade requests are not valid", 400);

            // Calculate total upgrade cost for all upgrades
            double totalUpgradeCost = CalculateTotalUpgradeCost(userArtefacts, datafile, request.Artefacts);

            UserCurrencies currencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (totalUpgradeCost <= 0 || totalUpgradeCost > currencies.PrestigePoints)
                return new("Cannot afford upgrade", 400);

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