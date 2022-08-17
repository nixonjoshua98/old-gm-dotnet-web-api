using MediatR;
using SRC.Common.Types;
using SRC.DataFiles.Cache;
using SRC.Mongo.Models;
using SRC.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.ArmouryHandlers
{
    public record UpgradeArmouryItemCommand(string UserID, int ItemID) : IRequest<Result<UpgradeArmouryItemResponse>>;

    public record UpgradeArmouryItemResponse(UserArmouryItem Item, int UpgradeCost);

    public class UpgradeArmouryItemHandler : IRequestHandler<UpgradeArmouryItemCommand, Result<UpgradeArmouryItemResponse>>
    {
        private readonly ArmouryService _armory;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;

        public UpgradeArmouryItemHandler(ArmouryService armoury, CurrenciesService currencies, IDataFileCache dataFiles)
        {
            _armory = armoury;
            _currencies = currencies;
            _dataFiles = dataFiles;
        }

        public async Task<Result<UpgradeArmouryItemResponse>> Handle(UpgradeArmouryItemCommand request, CancellationToken cancellationToken)
        {
            var datafile = _dataFiles.Armoury;

            var userItem = await _armory.GetArmouryItemAsync(request.UserID, request.ItemID);
            var itemData = datafile.FirstOrDefault(x => x.ID == request.ItemID);

            if (userItem is null || itemData is null)
                return new ServerError("Item is not valid", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            var upgradeCost = CalculateUpgradeCost(userItem);

            if (upgradeCost > userCurrencies.ArmouryPoints)
                return new ServerError("Cannot afford upgrade", 400);

            await _currencies.UpdateUserAsync(request.UserID, upd => upd.Inc(doc => doc.ArmouryPoints, -upgradeCost));

            var upgradedItem = await _armory.IncrementItemAsync(request.UserID, request.ItemID, new() { Level = 1 });

            return new UpgradeArmouryItemResponse(upgradedItem, upgradeCost);
        }

        private int CalculateUpgradeCost(UserArmouryItem item)
        {
            return item.Level + 5;
        }
    }
}
