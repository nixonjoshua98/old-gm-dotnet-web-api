using System.Collections.Generic;

namespace SRC.Core.BountyShop.Models
{
    public class BountyShopItems
    {
        public List<BountyShopCurrencyItem> CurrencyItems = new();
        public List<BountyShopArmouryItem> ArmouryItems = new();
    }
}
