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
        public string ShopItemID;
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

            var shopItem = userShop.ArmouryItems.First(x => x.ID == request.ShopItemID);

            var itemPurchases = await _bountyshop.GetDailyItemPurchasesAsync(request.UserID, request.ShopItemID, request.DailyRefresh);

            if (itemPurchases.Count >= 1)
                throw new ServerException("Item already purchased", 400);

            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (shopItem.PurchaseCost > userCurrencies.BountyPoints)
                throw new ServerException("Cannot afford purchase", 400);

            userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = -shopItem.PurchaseCost });

            await _bountyshop.InsertShopPurchaseAsync(new()
            {
                UserID = request.UserID,
                ItemID = request.ShopItemID,
                PurchaseTime = DateTime.UtcNow
            });

            var armouryItem = await InsertArmouryItemAsync(request, shopItem);

            return new PurchaseArmouryItemResponse(userCurrencies, armouryItem);
        }

        async Task<UserArmouryItem> InsertArmouryItemAsync(PurchaseArmouryItemRequest request, UserBountyShopArmouryItem item)
        {
            UserArmouryItem armouryItem = await _armoury.GetArmouryItemAsync(request.UserID, item.ItemID);

            // Increment the level by 1 if the user does not have the item (i.e set the level to 1 if the item is new to the user)
            return await _armoury.IncrementItemAsync(request.UserID, item.ItemID, new() { Owned = 1, Level = armouryItem is null ? 1 : 0 }, upsert: true);
        }
    }
}
