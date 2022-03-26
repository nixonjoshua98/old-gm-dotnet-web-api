using GMServer.Exceptions;
using GMServer.Services;
using GMServer.UserModels.UserModels;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.ArmouryHandlers
{
    public class UpgradeArmouryItemRequest : IRequest<UpgradeArmouryItemResponse>
    {
        public string UserID;
        public int ItemID;
    }

    public record UpgradeArmouryItemResponse(UserArmouryItem Item, int UpgradeCost);

    public class UpgradeArmouryItemHandler : IRequestHandler<UpgradeArmouryItemRequest, UpgradeArmouryItemResponse>
    {
        private readonly ArmouryService _armory;
        private readonly CurrenciesService _currencies;

        public UpgradeArmouryItemHandler(ArmouryService armoury, CurrenciesService currencies)
        {
            _armory = armoury;
            _currencies = currencies;
        }

        public async Task<UpgradeArmouryItemResponse> Handle(UpgradeArmouryItemRequest request, CancellationToken cancellationToken)
        {
            var datafile = _armory.GetDataFile();

            var userItem = await _armory.GetArmouryItemAsync(request.UserID, request.ItemID);
            var itemData = datafile.FirstOrDefault(x => x.ID == request.ItemID);

            if (userItem is null || itemData is null)
                throw new ServerException("Item is not valid", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            var upgradeCost = CalculateUpgradeCost(userItem);

            if (upgradeCost > userCurrencies.ArmouryPoints)
                throw new ServerException("Cannot afford upgrade", 400);

            await _currencies.IncrementAsync(request.UserID, new() { ArmouryPoints = -upgradeCost });

            var upgradedItem = await _armory.IncrementItemAsync(request.UserID, request.ItemID, new() { Level = 1 });

            return new UpgradeArmouryItemResponse(upgradedItem, upgradeCost);
        }

        int CalculateUpgradeCost(UserArmouryItem item)
        {
            return item.Level + 5;
        }
    }
}
