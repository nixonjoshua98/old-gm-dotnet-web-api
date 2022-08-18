using MediatR;
using MongoDB.Driver;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Context;
using SRC.Mongo;
using SRC.Mongo.Models;
using SRC.Services.BountyShop.Models;
using SRC.Services;
using SRC.Services.BountyShop;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.BountyShopHandler
{
    public record PurchaseCurrencyItemCommand(string UserID,
                                              string ItemID,
                                              int GameDayNumber) : IRequest<Result<PurchaseCurrencyItemResponse>>;

    public record PurchaseCurrencyItemResponse(UserCurrencies Currencies, long PurchaseCost);

    public class PurchaseCurrencyItemHandler : IRequestHandler<PurchaseCurrencyItemCommand, Result<PurchaseCurrencyItemResponse>>
    {
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly IBountyShopFactory _shopFactory;
        private readonly IMongoSessionFactory _mongoSession;

        public PurchaseCurrencyItemHandler(BountyShopService bountyshop, CurrenciesService currencies, IBountyShopFactory shopFactory, IMongoSessionFactory mongoSession)
        {
            _currencies = currencies;
            _bountyshop = bountyshop;
            _shopFactory = shopFactory;
            _mongoSession = mongoSession;
        }

        public async Task<Result<PurchaseCurrencyItemResponse>> Handle(PurchaseCurrencyItemCommand request, CancellationToken cancellationToken)
        {
            // Load user data
            (var userGeneratedShop, var shopPurchases, var userCurrencies) = await LoadUserData(request);

            UserBSCurrencyItem shopItem = userGeneratedShop.ShopItems.CurrencyItems.FirstOrDefault(x => x.ID == request.ItemID);

            // Validate the request
            if (!ValidateRequest(shopItem, shopPurchases, userCurrencies, out var error))
                return error;

            // Perform the purchase inside a transaction
            userCurrencies = await _mongoSession.RunInTransaction(async (session, ct) =>
            {
                await _bountyshop.AddShopPurchaseAsync(request.UserID, request.GameDayNumber, new(request.ItemID, BountyShopItemType.CurrencyItem));

                // Update the currencies
                return await _currencies.UpdateUserAsync(session, request.UserID, upd =>
                {
                    var update = upd
                        .Inc(doc => doc.BountyPoints, -shopItem.PurchaseCost);

                    ApplyUpdateDefinition(shopItem, ref update);

                    return update;
                });
            });

            return new PurchaseCurrencyItemResponse(userCurrencies, shopItem.PurchaseCost);
        }

        private async Task<(GeneratedBountyShop, BountyShopModel, UserCurrencies)> LoadUserData(PurchaseCurrencyItemCommand request)
        {
            var shopTask = _shopFactory.GenerateBountyShopAsync(request.UserID);
            var purchasesTask = _bountyshop.GetUserShopAsync(request.UserID, 0);
            var currenciesTask = _currencies.GetUserCurrenciesAsync(request.UserID);

            await Task.WhenAll(shopTask, purchasesTask, currenciesTask);

            return (shopTask.Result, purchasesTask.Result ?? new(), currenciesTask.Result);
        }

        private bool ValidateRequest(UserBSCurrencyItem shopItem, BountyShopModel bountyShop, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            if (shopItem is null)
                error = new("Item not found", 404);

            else if (bountyShop.GetPurchase(BountyShopItemType.CurrencyItem, shopItem.ID) is not null)
                error = new("Item already purchased", 400);

            else if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        private void ApplyUpdateDefinition(UserBSCurrencyItem item, ref MongoDB.Driver.UpdateDefinition<UserCurrencies> update)
        {
            switch (item.CurrencyType)
            {
                case CurrencyType.ArmouryPoints:
                    update.Inc(doc => doc.ArmouryPoints, item.Quantity);
                    break;

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
