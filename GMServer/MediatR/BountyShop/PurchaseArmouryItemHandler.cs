using MediatR;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Mongo;
using SRC.Mongo.Models;
using SRC.Services;
using SRC.Services.BountyShop;
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
        private readonly BountyShopService _bountyshop;
        private readonly CurrenciesService _currencies;
        private readonly ArmouryService _armoury;
        private readonly IBountyShopFactory _shopFactory;
        private readonly IMongoSessionFactory _mongoSession;

        public PurchaseArmouryItemHandler(BountyShopService bountyshop,
                                          CurrenciesService currencies,
                                          ArmouryService armoury,
                                          IBountyShopFactory shopFactory,
                                          IMongoSessionFactory mongoSession)
        {
            _currencies = currencies;
            _armoury = armoury;
            _bountyshop = bountyshop;
            _shopFactory = shopFactory;
            _mongoSession = mongoSession;
        }

        private bool ValidateRequest(UserBSArmouryItem shopItem, BountyShopModel shop, UserCurrencies currencies, out ServerError error)
        {
            error = default;

            if (shopItem is null)
                error = new("Item not found", 404);

            // Item has already been purchased
            else if (shop.GetPurchase(BountyShopItemType.ArmouryItem, shopItem.ID) is not null)
                error = new("Item already purchased", 400);

            // User cannot afford the purchase
            else if (shopItem.PurchaseCost > currencies.BountyPoints)
                error = new("Cannot afford purchase", 400);

            return error == default;
        }

        public async Task<Result<PurchaseArmouryItemResponse>> Handle(PurchaseArmouryItemCommand request, CancellationToken cancellationToken)
        {
            var userShop    = await _shopFactory.GenerateBountyShopAsync(request.UserID);
            var shopState   = await _bountyshop.GetUserShopAsync(request.UserID, userShop.GameDayNumber) ?? new();


            var shopItem = userShop.ShopItems.ArmouryItems.FirstOrDefault(x => x.ID == request.ItemID);

            UserCurrencies userCurrencies = await _currencies.GetUserCurrenciesAsync(request.UserID);

            if (!ValidateRequest(shopItem, shopState, userCurrencies, out var error))
                return error;

            // Perform the purchase inside a transaction
            userCurrencies = await _mongoSession.RunInTransaction(async (session, ct) =>
            {
                return await _currencies.UpdateUserAsync(session, request.UserID, upd => upd.Inc(doc => doc.BountyPoints, -shopItem.PurchaseCost));
            });

            await _bountyshop.AddShopPurchaseAsync(request.UserID, request.GameDayNumber, new(request.ItemID, BountyShopItemType.ArmouryItem));

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
