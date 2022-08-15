using GMServer.Common;
using GMServer.Common.Types;
using GMServer.Context;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyShopHandler
{
    public record PurchaseCurrencyItemCommand(string UserID,
                                              string ItemID,
                                              List<UserBSCurrencyItem> ShopCurrencyItems,
                                              CurrentServerRefresh<IDailyRefresh> DailyRefresh) : IRequest<Result<PurchaseCurrencyItemResponse>>;

    public record PurchaseCurrencyItemResponse(UserCurrencies Currencies, long PurchaseCost);

    public class PurchaseCurrencyItemHandler : IRequestHandler<PurchaseCurrencyItemCommand, Result<PurchaseCurrencyItemResponse>>
    {
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;

        public PurchaseCurrencyItemHandler(BountyShopService bountyshop, CurrenciesService currencies)
        {
            _currencies = currencies;
            _bountyshop = bountyshop;
        }

        public async Task<Result<PurchaseCurrencyItemResponse>> Handle(PurchaseCurrencyItemCommand request, CancellationToken cancellationToken)
        {
            UserBSCurrencyItem shopItem = request.ShopCurrencyItems.First(x => x.ID == request.ItemID);
            BountyShopPurchase purchasedItem = await _bountyshop.GetPurchasedItemAsync(request.UserID, request.ItemID, request.DailyRefresh);
            UserCurrencies userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (!ValidateRequest(shopItem, purchasedItem, userCurrencies, out var error))
                return error;

            // Create the update model (includes the purchase cost decrement)
            var incrModel = CreateUpdateModel(shopItem);

            // Add the purchase log to the database
            await _bountyshop.InsertShopPurchaseAsync(new(request.UserID, request.ItemID, DateTime.UtcNow));

            // Add/remove the currencies
            var updateUserCurrencies = await _currencies.IncrementAsync(request.UserID, incrModel);

            return new PurchaseCurrencyItemResponse(updateUserCurrencies, shopItem.PurchaseCost);
        }

        private bool ValidateRequest(UserBSCurrencyItem shopItem, BountyShopPurchase itemPurchase, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            if (itemPurchase is not null)
                error = new("Item already purchased", 400);

            if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        private UserCurrencies CreateUpdateModel(UserBSCurrencyItem item)
        {
            return item.CurrencyType switch
            {
                CurrencyType.ArmouryPoints => new() { ArmouryPoints = item.Quantity, BountyPoints = -item.PurchaseCost },
                _ => throw new NotImplementedException(),
            };
        }
    }
}
