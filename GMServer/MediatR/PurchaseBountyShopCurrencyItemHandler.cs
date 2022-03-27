using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class PurchaseBountyShopCurrencyItemRequest : IRequest<PurchaseBountyShopCurrencyItemResponse>
    {
        public string UserID;
        public string ItemID;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public record PurchaseBountyShopCurrencyItemResponse(UserCurrencies Currencies, long PurchaseCost);

    public class PurchaseBountyShopCurrencyItemHandler : IRequestHandler<PurchaseBountyShopCurrencyItemRequest, PurchaseBountyShopCurrencyItemResponse>
    {
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;

        public PurchaseBountyShopCurrencyItemHandler(BountyShopService bountyshop, CurrenciesService currencies)
        {
            _currencies = currencies;
            _bountyshop = bountyshop;
        }

        public async Task<PurchaseBountyShopCurrencyItemResponse> Handle(PurchaseBountyShopCurrencyItemRequest request, CancellationToken cancellationToken)
        {
            var userShop = _bountyshop.GetUserBountyShop(request.UserID, request.DailyRefresh);

            var shopItem = userShop.CurrencyItems.First(x => x.ID == request.ItemID);

            var itemPurchases = await _bountyshop.GetDailyItemPurchasesAsync(request.UserID, request.ItemID, request.DailyRefresh);

            if (itemPurchases.Count >= 1)
                throw new ServerException("Item already purchased", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                throw new ServerException("Cannot afford purchase", 400);

            // Create the update model (includes the purchase cost decrement)
            var incrModel = CreateUpdateModel(shopItem);

            await _bountyshop.InsertShopPurchaseAsync(new()
            {
                UserID = request.UserID,
                ItemID = request.ItemID,
                PurchaseTime = DateTime.UtcNow
            });

            var updateUserCurrencies = await _currencies.IncrementAsync(request.UserID, incrModel);

            return new PurchaseBountyShopCurrencyItemResponse(updateUserCurrencies, shopItem.PurchaseCost);
        }

        private UserCurrencies CreateUpdateModel(UserBountyShopCurrencyItem item)
        {
            return item.CurrencyType switch
            {
                Common.CurrencyType.ArmouryPoints => new() { ArmouryPoints = item.Quantity, BountyPoints = -item.PurchaseCost },
                _ => throw new NotImplementedException(),
            };
        }
    }
}
