using GMServer.Common;
using GMServer.Context;
using GMServer.LootTable;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.BountyShopHandler
{
    public class GetUserBountyShopRequest : IRequest<GetUserBountyShopResponse>
    {
        public string UserID;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public class GetUserBountyShopResponse
    {
        public DateTime ShopCreationTime;
        public List<BountyShopPurchase> Purchases;
        public BountyShopItems ShopItems;
    }

    public class GetUserBountyShopHandler : IRequestHandler<GetUserBountyShopRequest, GetUserBountyShopResponse>
    {
        private readonly BountyShopService _bountyshop;
        private readonly ArmouryService _armoury;

        public GetUserBountyShopHandler(BountyShopService bountyshop, ArmouryService armoury)
        {
            _armoury = armoury;
            _bountyshop = bountyshop;
        }

        public async Task<GetUserBountyShopResponse> Handle(GetUserBountyShopRequest request, CancellationToken cancellationToken)
        {
            RDSTable root = new();

            root.AddEntry(CreateArmouryItemsLootTable());
            root.AddEntry(AddCurrencyItemsTable());

            BountyShopItems items = GetItems(root, 4, $"{request.UserID}-1{request.DailyRefresh.Previous}");

            var shopPurchases = await _bountyshop.GetDailyPurchasesAsync(request.UserID, request.DailyRefresh);

            return new GetUserBountyShopResponse
            {
                ShopCreationTime = DateTime.UtcNow,
                ShopItems = items,
                Purchases = shopPurchases
            };
        }

        public BountyShopItems GetItems(RDSTable lootTable, int count, string seed)
        {
            Random rnd = Utility.SeededRandom(seed);

            BountyShopItems shop = new();

            var results = lootTable.GetResults(count, rnd);

            for (int i = 0; i < results.Count; i++)
            {
                var current = results[i];

                if (current is RDSValue<BountyShopCurrencyItem> cItem)
                {
                    shop.CurrencyItems.Add(new()
                    {
                        ID = $"CI{rnd.NextInt64()}",
                        Quantity = cItem.Value.Quantity,
                        CurrencyType = cItem.Value.CurrencyType,
                        PurchaseCost = cItem.Value.PurchaseCost,
                    });
                }
                else if (current is RDSValue<BountyShopArmouryItem> aItem)
                {
                    shop.ArmouryItems.Add(new()
                    {
                        ID = $"AI{rnd.NextInt64()}",
                        ItemID = aItem.Value.ID,
                        PurchaseCost = aItem.Value.PurchaseCost
                    });
                }
            }

            return shop;
        }

        RDSTable AddCurrencyItemsTable()
        {
            BountyShopDataFile shopDataFile = _bountyshop.GetDataFile();

            RDSTable table = new()
            {
                Always = shopDataFile.CurrencyItems.Always,
                Unique = shopDataFile.CurrencyItems.Unique,
                Weight = shopDataFile.CurrencyItems.Weight,
            };

            foreach (BountyShopCurrencyItem item in shopDataFile.CurrencyItems.Items)
            {
                RDSValue<BountyShopCurrencyItem> itemValue = new(item)
                {
                    Always = item.Always,
                    Unique = item.Unique,
                    Weight = item.Weight,
                };

                table.AddEntry(itemValue);
            }

            return table;
        }

        RDSTable CreateArmouryItemsLootTable()
        {
            BountyShopDataFile shopDataFile = _bountyshop.GetDataFile();
            List<ArmouryItem> armouryItems = _armoury.GetDataFile();

            RDSTable table = new()
            {
                Always = shopDataFile.ArmouryItems.Always,
                Unique = shopDataFile.ArmouryItems.Unique,
                Weight = shopDataFile.ArmouryItems.Weight,
            };

            foreach (ArmouryItem item in armouryItems)
            {
                BountyShopArmouryItem bsItem = new()
                {
                    ID = item.ID,
                    PurchaseCost = shopDataFile.ArmouryItems.PurchaseCost
                };

                RDSValue<BountyShopArmouryItem> rdsItem = new(bsItem)
                {

                };

                table.AddEntry(rdsItem);
            }

            return table;
        }
    }
}
