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
    public record PurchaseArmouryItemCommand(string UserID,
                                             string ShopItemID,
                                             List<UserBSArmouryItem> ShopArmouryItems,
                                             CurrentServerRefresh<IDailyRefresh> DailyRefresh) : IRequest<Result<PurchaseArmouryItemResponse>>;

    public record PurchaseArmouryItemResponse(UserArmouryItem ArmouryItem, UserCurrencies Currencies);

    public class PurchaseArmouryItemHandler : IRequestHandler<PurchaseArmouryItemCommand, Result<PurchaseArmouryItemResponse>>
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

        private bool ValidateRequest(UserBSArmouryItem shopItem, BountyShopPurchase shopPurchase, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            // Item has already been purchased
            if (shopPurchase is not null)
                error = new("Item already purchased", 400);

            // User cannot afford the purchase
            else if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        public async Task<Result<PurchaseArmouryItemResponse>> Handle(PurchaseArmouryItemCommand request, CancellationToken cancellationToken)
        {
            UserBSArmouryItem shopItem = request.ShopArmouryItems.First(x => x.ID == request.ShopItemID);

            BountyShopPurchase? purchasedItem = await _bountyshop.GetPurchasedItemAsync(request.UserID, request.ShopItemID, request.DailyRefresh);
            UserCurrencies userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (!ValidateRequest(shopItem, purchasedItem, userCurrencies, out var error))
                return error;

            userCurrencies = await _currencies.IncrementAsync(request.UserID, new() { BountyPoints = -shopItem.PurchaseCost });

            await _bountyshop.InsertShopPurchaseAsync(new(request.UserID, request.ShopItemID, DateTime.UtcNow));

            var armouryItem = await InsertArmouryItemAsync(request, shopItem);

            return new PurchaseArmouryItemResponse(armouryItem, userCurrencies);
        }

        private async Task<UserArmouryItem> InsertArmouryItemAsync(PurchaseArmouryItemCommand request, UserBSArmouryItem item)
        {
            UserArmouryItem armouryItem = await _armoury.GetArmouryItemAsync(request.UserID, item.ItemID);

            // Increment the level by 1 if the user does not have the item (i.e set the level to 1 if the item is new to the user)
            return await _armoury.IncrementItemAsync(request.UserID, item.ItemID, new() { Owned = 1, Level = armouryItem is null ? 1 : 0 }, upsert: true);
        }
    }
}
