using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.BountyShopHandler;
using GMServer.Models.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BountyShopController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly RequestContext _context;

        public BountyShopController(IMediator mediator, RequestContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resp = await GetUserBountyShop();

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetBountyShop");
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPut("Purchase/Currency")]
        [Authorize]
        public async Task<IActionResult> PurchaseCurrency(PurchaseBountyShopItemBody body)
        {
            try
            {
                var userShop = await GetUserBountyShop();

                var resp = await _mediator.Send(new PurchaseCurrencyItemRequest
                {
                    UserID = User.UserID(),
                    ItemID = body.ItemID,
                    ShopCurrencyItems = userShop.ShopItems.CurrencyItems,
                    DailyRefresh = _context.DailyRefresh
                });

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PurchaseCurrency");
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPut("Purchase/ArmouryItem")]
        [Authorize]
        public async Task<IActionResult> PurchaseArmouryItem(PurchaseBountyShopItemBody body)
        {
            try
            {
                var userShop = await GetUserBountyShop();

                var resp = await _mediator.Send(new PurchaseArmouryItemRequest
                {
                    UserID = User.UserID(),
                    ShopItemID = body.ItemID,
                    ShopArmouryItems = userShop.ShopItems.ArmouryItems,
                    DailyRefresh = _context.DailyRefresh
                });

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PurchaseArmouryItem");
                return new InternalServerError(ex.Message);
            }
        }

        /// <summary>
        /// General mediator request for the current user bounty shop (used in most requests)
        /// </summary>
        async Task<GetUserBountyShopResponse> GetUserBountyShop()
        {
            return await _mediator.Send(new GetUserBountyShopRequest { DailyRefresh = _context.DailyRefresh, UserID = User.UserID() });
        }
    }
}