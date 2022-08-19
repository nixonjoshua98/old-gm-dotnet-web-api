using MediatR;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Core.BountyShop;
using SRC.Core.BountyShop.Models;
using SRC.Mongo;
using SRC.Mongo.Models;
using SRC.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.BountyShopHandler
{
    public record PurchaseArmouryItemCommand(string UserID,
                                             string ItemID,
                                             int GameDayNumber) : IRequest<Result<PurchaseArmouryItemResponse>>;

    public record PurchaseArmouryItemResponse(UserArmouryItem ArmouryItem, UserCurrencies Currencies);

    public class PurchaseArmouryItemHandler : IRequestHandler<PurchaseArmouryItemCommand, Result<PurchaseArmouryItemResponse>>
    {
        private readonly IBountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly ArmouryService _armoury;
        private readonly IMongoSessionFactory _mongoSession;

        public PurchaseArmouryItemHandler(IBountyShopService bountyshop,
                                          CurrenciesService currencies,
                                          ArmouryService armoury,
                                          IMongoSessionFactory mongoSession)
        {
            _currencies = currencies;
            _armoury = armoury;
            _bountyshop = bountyshop;
            _mongoSession = mongoSession;
        }

        private bool ValidateRequest(GeneratedBountyShop bountyShop, BountyShopArmouryItem shopItem, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            if (shopItem is null)
                error = new("Item not found", 404);

            // Item has already been purchased
            else if (bountyShop.Purchases.FirstOrDefault(x => x.ItemType == BountyShopItemType.ArmouryItem && x.ItemID == shopItem.ID) is not null)
                error = new("Item already purchased", 400);

            // User cannot afford the purchase
            else if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        public async Task<Result<PurchaseArmouryItemResponse>> Handle(PurchaseArmouryItemCommand request, CancellationToken cancellationToken)
        {
            var userShop = await _bountyshop.GetUserShopAsync(request.UserID, request.GameDayNumber);
            var userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            var shopItem = userShop.ShopItems.ArmouryItems.FirstOrDefault(x => x.ID == request.ItemID);

            if (!ValidateRequest(userShop, shopItem, userCurrencies, out var error))
                return error;

            userCurrencies = await _mongoSession.RunInTransaction(async (session, ct) =>
            {
                await _bountyshop.AddShopPurchaseAsync(session,
                                                       request.UserID,
                                                       request.GameDayNumber,
                                                       new(request.ItemID, BountyShopItemType.ArmouryItem));

                return await _currencies.UpdateUserAsync(session,
                                                         request.UserID,
                                                         upd => upd.Inc(doc => doc.BountyPoints, -shopItem.PurchaseCost));
            });

            var armouryItem = await InsertArmouryItemAsync(request, shopItem);

            return new PurchaseArmouryItemResponse(armouryItem, userCurrencies);
        }

        private async Task<UserArmouryItem> InsertArmouryItemAsync(PurchaseArmouryItemCommand request, BountyShopArmouryItem item)
        {
            UserArmouryItem armouryItem = await _armoury.GetArmouryItemAsync(request.UserID, item.ItemID);

            // Increment the level by 1 if the user does not have the item (i.e set the level to 1 if the item is new to the user)
            return await _armoury.IncrementItemAsync(request.UserID, item.ItemID, new() { Owned = 1, Level = armouryItem is null ? 1 : 0 }, upsert: true);
        }
    }
}
