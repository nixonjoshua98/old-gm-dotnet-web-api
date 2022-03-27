using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyShopHandler
{
    public class PurchaseArmouryItemRequest : IRequest<PurchaseArmouryItemResponse>
    {
        public string UserID;
        public string ItemID;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public record PurchaseArmouryItemResponse(UserCurrencies Currencies, UserArmouryItem ArmouryItem);

    public class PurchaseArmouryItemHandler : IRequestHandler<PurchaseArmouryItemRequest, PurchaseArmouryItemResponse>
    {
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly ArmouryService _armoury;

        public PurchaseArmouryItemHandler(BountyShopService bountyshop, CurrenciesService currencies, ArmouryService armoury)
        {
            _currencies = currencies;
            _armoury = armoury;
            _bountyshop = bountyshop;
        }

        public async Task<PurchaseArmouryItemResponse> Handle(PurchaseArmouryItemRequest request, CancellationToken cancellationToken)
        {
            var userShop = _bountyshop.GetUserBountyShop(request.UserID, request.DailyRefresh);

            var shopItem = userShop.ArmouryItems.First(x => x.ID == request.ItemID);

            var itemPurchases = await _bountyshop.GetDailyItemPurchasesAsync(request.UserID, request.ItemID, request.DailyRefresh);

            if (itemPurchases.Count >= 1)
                throw new ServerException("Item already purchased", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                throw new ServerException("Cannot afford purchase", 400);

            userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = -shopItem.PurchaseCost });

            await _bountyshop.InsertShopPurchaseAsync(new()
            {
                UserID = request.UserID,
                ItemID = request.ItemID,
                PurchaseTime = DateTime.UtcNow
            });

            var armouryItem = await _armoury.IncrementItemAsync(request.UserID, shopItem.ItemID, new() { Owned = 1 }, upsert: true);

            return new PurchaseArmouryItemResponse(userCurrencies, armouryItem);
        }
    }
}
