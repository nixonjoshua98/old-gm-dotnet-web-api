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
    public class PurchaseArmouryItemRequest : IRequest<PurchaseArmouryItemResponse>
    {
        public string UserID;
        public string ShopItemID;
        public List<UserBSArmouryItem> ShopArmouryItems;
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;
    }

    public class PurchaseArmouryItemResponse : AbstractResponseWithError
    {
        public UserCurrencies Currencies;
        public UserArmouryItem ArmouryItem;

        public PurchaseArmouryItemResponse() {}
        public PurchaseArmouryItemResponse(string message, int code) : base(message, code) { }
        public static PurchaseArmouryItemResponse AsError(string m, int code) => new(m, code);
    }

    public class PurchaseArmouryItemHandler : IRequestHandler<PurchaseArmouryItemRequest, PurchaseArmouryItemResponse>
    {
        private BountyShopService _bountyshop;
        private CurrenciesService _currencies;
        private ArmouryService _armoury;

        public PurchaseArmouryItemHandler(BountyShopService bountyshop, CurrenciesService currencies, ArmouryService armoury)
        {
            _currencies = currencies;
            _armoury = armoury;
            _bountyshop = bountyshop;
        }

        public async Task<PurchaseArmouryItemResponse> Handle(PurchaseArmouryItemRequest request, CancellationToken cancellationToken)
        {
            var shopItem = request.ShopArmouryItems.First(x => x.ID == request.ShopItemID);

            var purchasedItem = await _bountyshop.GetPurchasedItemAsync(request.UserID, request.ShopItemID, request.DailyRefresh);

            if (purchasedItem is not null) // Item has already been purchased
                return PurchaseArmouryItemResponse.AsError("Item already purchased", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                return new("Cannot afford purchase", 400);

            userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = -shopItem.PurchaseCost });

            await InsertShopPurchase(request.UserID, request.ShopItemID);

            var armouryItem = await InsertArmouryItemAsync(request, shopItem);

            return new PurchaseArmouryItemResponse { ArmouryItem = armouryItem, Currencies = userCurrencies };
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

        private async Task<UserArmouryItem> InsertArmouryItemAsync(PurchaseArmouryItemRequest request, UserBSArmouryItem item)
        {
            UserArmouryItem armouryItem = await _armoury.GetArmouryItemAsync(request.UserID, item.ItemID);

            // Increment the level by 1 if the user does not have the item (i.e set the level to 1 if the item is new to the user)
            return await _armoury.IncrementItemAsync(request.UserID, item.ItemID, new() { Owned = 1, Level = armouryItem is null ? 1 : 0 }, upsert: true);
        }
    }
}
