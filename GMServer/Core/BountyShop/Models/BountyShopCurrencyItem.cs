using SRC.Common.Enums;

namespace SRC.Core.BountyShop.Models
{
    public record BountyShopCurrencyItem(string ID, CurrencyType CurrencyType, int Quantity, int PurchaseCost);
}
