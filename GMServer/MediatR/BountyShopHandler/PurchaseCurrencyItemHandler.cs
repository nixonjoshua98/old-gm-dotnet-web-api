using GMServer.Common;
using GMServer.Context;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyShopHandler
{
    public class PurchaseCurrencyItemRequest : IRequest<PurchaseCurrencyItemResponse>
    {
        public string UserID;
        public string ItemID;
        public List<UserBSCurrencyItem> ShopCurrencyItems;
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;
    }

    public class PurchaseCurrencyItemResponse : AbstractResponseWithError
    {
        public UserCurrencies Currencies;
        public long PurchaseCost;

        public PurchaseCurrencyItemResponse() { }

        public PurchaseCurrencyItemResponse(string message, int code) : base(message, code) { }
    }

    public class PurchaseCurrencyItemHandler : IRequestHandler<PurchaseCurrencyItemRequest, PurchaseCurrencyItemResponse>
    {
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;

        public PurchaseCurrencyItemHandler(BountyShopService bountyshop, CurrenciesService currencies)
        {
            _currencies = currencies;
            _bountyshop = bountyshop;
        }

        public async Task<PurchaseCurrencyItemResponse> Handle(PurchaseCurrencyItemRequest request, CancellationToken cancellationToken)
        {
            var shopItem = request.ShopCurrencyItems.First(x => x.ID == request.ItemID);

            // Verify the item is in stock
            var purchasedItem = await _bountyshop.GetPurchasedItemAsync(request.UserID, request.ItemID, request.DailyRefresh);

            if (purchasedItem is not null)
                return new("Item already purchased", 400);

            // Verify that the user can afford the purchase
            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                return new("Cannot afford purchase", 400);

            // Create the update model (includes the purchase cost decrement)
            var incrModel = CreateUpdateModel(shopItem);

            // Add the purchase log to the database
            await InsertShopPurchase(request.UserID, request.ItemID);

            // Add/remove the currencies
            var updateUserCurrencies = await _currencies.IncrementAsync(request.UserID, incrModel);

            return new() { Currencies = updateUserCurrencies, PurchaseCost = shopItem.PurchaseCost };
        }

        async Task InsertShopPurchase(string userId, string itemId)
        {
            await _bountyshop.InsertShopPurchaseAsync(new()
            {
                UserID = userId,
                ItemID = itemId,
                PurchaseTime = DateTime.UtcNow
            });
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
