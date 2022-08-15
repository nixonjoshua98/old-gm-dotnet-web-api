using GMServer.Caching.DataFiles.Models;
using GMServer.Common;
using GMServer.Common.Types;
using GMServer.Models.RequestModels;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using SRC.DataFiles.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.Artefacts
{
    public record BulkUpgradeArtefactCommand(string UserID, List<UserArtefactUpgrade> Artefacts) : IRequest<Result<BulkUpgradeArtefactResponse>>;

    public record BulkUpgradeArtefactResponse(List<UserArtefact> Artefacts, double UpgradeCost, double RemainingPrestigePoints);

    public class BulkUpgradeArtefactHandler : IRequestHandler<BulkUpgradeArtefactCommand, Result<BulkUpgradeArtefactResponse>>
    {
        private readonly ArtefactsService _artefacts;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;

        public BulkUpgradeArtefactHandler(ArtefactsService artefacts, CurrenciesService currencies, IDataFileCache dataFiles)
        {
            _currencies = currencies;
            _artefacts = artefacts;
            _dataFiles = dataFiles;
        }

        private bool ValidateRequest(BulkUpgradeArtefactCommand request, List<UserArtefact> ownedArtefacts, out ServerError error)
        {
            error = default;

            var datafile = _dataFiles.Artefacts;

            if (request.Artefacts.Count == 0)
                error = new ServerError("No artefacts provided", 400);

            else if (!AllUniqueArtefacts(request.Artefacts))
                error = new ServerError("Duplicate artefacts found", 400);

            else if (!UserOwnsAllArtefacts(ownedArtefacts, request.Artefacts))
                error = new ServerError("Invalid artefact found", 400);

            else if (!UpgradeLevelsValid(ownedArtefacts, datafile, request.Artefacts))
                error = new ServerError("Some artefact upgrade requests are not valid", 400);

            return error == default;
        }

        public async Task<Result<BulkUpgradeArtefactResponse>> Handle(BulkUpgradeArtefactCommand request, CancellationToken cancellationToken)
        {
            List<UserArtefact> userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            var datafile = _dataFiles.Artefacts;

            // Perform early request validation and exit early if it does not pass
            if (!ValidateRequest(request, userArtefacts, out var error))
                return error;

            // Calculate total upgrade cost for all upgrades
            double totalUpgradeCost = CalculateTotalUpgradeCost(userArtefacts, datafile, request.Artefacts);

            UserCurrencies currencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (totalUpgradeCost <= 0 || totalUpgradeCost > currencies.PrestigePoints)
                return new ServerError("Cannot afford upgrade", 400);

            // Decrement the upgrade cost and get the new values
            var userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { PrestigePoints = -totalUpgradeCost });

            // Perform the update on the database
            await _artefacts.BulkUpgradeArtefacts(request.UserID, request.Artefacts);

            // Re-fetch the updated artefacts
            userArtefacts = await _artefacts.GetUserArtefactsAsync(request.UserID);

            return new BulkUpgradeArtefactResponse(userArtefacts, totalUpgradeCost, userCurrencies.PrestigePoints);
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
    }
}