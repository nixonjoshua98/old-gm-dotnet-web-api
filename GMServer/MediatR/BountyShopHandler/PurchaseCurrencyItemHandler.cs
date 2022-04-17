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
        public List<UserBountyShopCurrencyItem> ShopCurrencyItems;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public class PurchaseCurrencyItemResponse : AbstractResponseWithError
    {
        public UserCurrencies Currencies;
        public long PurchaseCost;

        public PurchaseCurrencyItemResponse(string message, int code) : base(message, code)
        {
        }

        public PurchaseCurrencyItemResponse(UserCurrencies currencies, long purchaseCost)
        {
            Currencies = currencies;
            PurchaseCost = purchaseCost;
        }
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

            var itemPurchases = await _bountyshop.GetDailyItemPurchasesAsync(request.UserID, request.ItemID, request.DailyRefresh);

            if (itemPurchases.Count >= 1)
                return new("Item already purchased", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                return new("Cannot afford purchase", 400);

            // Create the update model (includes the purchase cost decrement)
            var incrModel = CreateUpdateModel(shopItem);

            await _bountyshop.InsertShopPurchaseAsync(new BountyShopPurchase
            {
                UserID = request.UserID,
                ItemID = request.ItemID,
                PurchaseTime = DateTime.UtcNow
            });

            var updateUserCurrencies = await _currencies.IncrementAsync(request.UserID, incrModel);

            return new PurchaseCurrencyItemResponse(updateUserCurrencies, shopItem.PurchaseCost);
        }

        private UserCurrencies CreateUpdateModel(UserBountyShopCurrencyItem item)
        {
            return item.CurrencyType switch
            {
                CurrencyType.ArmouryPoints => new() { ArmouryPoints = item.Quantity, BountyPoints = -item.PurchaseCost },
                _ => throw new NotImplementedException(),
            };
        }
    }
}
