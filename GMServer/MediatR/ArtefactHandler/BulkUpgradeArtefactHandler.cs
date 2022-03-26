using GMServer.Common;
using GMServer.Exceptions;
using GMServer.Models.RequestModels;
using GMServer.Services;
using GMServer.UserModels.DataFileModels;
using GMServer.UserModels.UserModels;
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
            if (request.Artefacts.Count == 0)
                throw new ServerException("No artefacts provided", 400);

            else if (!IsUniqueArtefacts(request.Artefacts))
                throw new ServerException("Duplicate artefacts found", 400);

            var userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            if (!UserOwnsAllArtefacts(userArtefacts, request.Artefacts))
                throw new ServerException("Invalid artefacts found", 400);

            var datafile = _artefacts.GetDataFile();

            var upgradeDict = CreateUpgradeCostDictionary(userArtefacts, datafile, request.Artefacts);

            double totalUpgradeCost = upgradeDict.Values.Sum();

            UserCurrencies currencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (totalUpgradeCost > currencies.PrestigePoints)
                throw new ServerException("Cannot afford upgrade", 400);

            var userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = -totalUpgradeCost });

            await _artefacts.BulkUpgradeArtefacts(request.UserID, request.Artefacts);

            // Re-fetch the updated artefacts
            userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            return new BulkUpgradeArtefactResponse(userArtefacts, totalUpgradeCost, userCurrencies.PrestigePoints);
        }

        bool UserOwnsAllArtefacts(List<UserArtefact> unlockedArtefacts, List<UserArtefactUpgrade> artefacts)
        {
            IEnumerable<int> unlockedArtefactsId = unlockedArtefacts.Select(x => x.ArtefactID);

            return artefacts.All(art => unlockedArtefactsId.Contains(art.ArtefactID));
        }

        bool IsUniqueArtefacts(List<UserArtefactUpgrade> artefacts)
        {
            return artefacts.Select(x => x.ArtefactID).ToHashSet().Count() == artefacts.Count;
        }

        Dictionary<int, double> CreateUpgradeCostDictionary(List<UserArtefact> unlockedArtefacts, List<Artefact> artefactsDatafile, List<UserArtefactUpgrade> artefactUpgrades)
        {
            Dictionary<int, double> upgradeCostDictionary = new Dictionary<int, double>();

            foreach (var artUpgrade in artefactUpgrades)
            {
                Artefact artefact = artefactsDatafile.FirstOrDefault(x => x.ID == artUpgrade.ArtefactID);
                UserArtefact userArtefact = unlockedArtefacts.FirstOrDefault(x => x.ArtefactID == artUpgrade.ArtefactID);

                if (artefact is null || userArtefact is null)
                    continue;

                upgradeCostDictionary.Add(artUpgrade.ArtefactID, CalculateUpgradeCost(userArtefact, artefact, artUpgrade.Levels));
            }

            return upgradeCostDictionary;
        }

        double CalculateUpgradeCost(UserArtefact userArtefact, Artefact artefact, int levels)
        {
            return artefact.CostCoeff * GameFormulas.SumNonIntegerPowerSeq(userArtefact.Level, levels, artefact.CostExpo);
        }

    }
}
