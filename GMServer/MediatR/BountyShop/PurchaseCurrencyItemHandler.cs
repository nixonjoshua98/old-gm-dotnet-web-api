using MediatR;
using MongoDB.Driver;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Core.BountyShop;
using SRC.Core.BountyShop.Models;
using SRC.Mongo;
using SRC.Mongo.Models;
using SRC.Services;
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
        private readonly IBountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly IMongoSessionFactory _mongoSession;

        public PurchaseCurrencyItemHandler(IBountyShopService bountyshop, CurrenciesService currencies, IMongoSessionFactory mongoSession)
        {
            _currencies = currencies;
            _bountyshop = bountyshop;
            _mongoSession = mongoSession;
        }

        public async Task<Result<PurchaseCurrencyItemResponse>> Handle(PurchaseCurrencyItemCommand request, CancellationToken cancellationToken)
        {
            var userShop = await _bountyshop.GetUserShopAsync(request.UserID, request.GameDayNumber);
            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            BountyShopCurrencyItem shopItem = userShop.ShopItems.CurrencyItems.FirstOrDefault(x => x.ID == request.ItemID);

            // Validate the request
            if (!ValidateRequest(userShop, shopItem, userCurrencies, out var error))
                return error;

            // Perform the purchase inside a transaction
            userCurrencies = await _mongoSession.RunInTransaction(async (session, ct) =>
            {
                await _bountyshop.AddShopPurchaseAsync(session,
                                                       request.UserID,
                                                       request.GameDayNumber,
                                                       new(request.ItemID, BountyShopItemType.CurrencyItem));

                // Update the currencies
                return await _currencies.UpdateUserAsync(session, request.UserID, upd =>
                {
                    var update = upd
                        .Inc(doc => doc.BountyPoints, -shopItem.PurchaseCost);

                    return ApplyUpdateDefinition(shopItem, update);
                });
            });

            return new PurchaseCurrencyItemResponse(userCurrencies, shopItem.PurchaseCost);
        }

        private bool ValidateRequest(GeneratedBountyShop userShop, BountyShopCurrencyItem shopItem, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            if (shopItem is null)
                error = new("Item not found", 404);

            else if (userShop.Purchases.FirstOrDefault(x => x.ItemType == BountyShopItemType.CurrencyItem && x.ItemID == shopItem.ID) is not null)
                error = new("Item already purchased", 400);

            else if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        private UpdateDefinition<UserCurrencies> ApplyUpdateDefinition(BountyShopCurrencyItem item, UpdateDefinition<UserCurrencies> update)
        {
            return item.CurrencyType switch
            {
                CurrencyType.ArmouryPoints => update.Inc(doc => doc.ArmouryPoints, item.Quantity),
                CurrencyType.Gemstones => update.Inc(doc => doc.Gemstones, item.Quantity),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
