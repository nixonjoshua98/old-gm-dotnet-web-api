using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR;
using GMServer.Models.RequestModels;
using GMServer.Services;
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
        private readonly BountyShopService _bountyshop;

        public BountyShopController(IMediator mediator, BountyShopService bountyshop)
        {
            _bountyshop = bountyshop;
            _mediator = mediator;
        }

        [HttpPut("Purchase/Currency")]
        [Authorize]
        public async Task<IActionResult> PurchaseCurrency(PurchaseBountyShopItemBody body, [FromServices] RequestContext context)
        {
            try
            {
                var resp = await _mediator.Send(new PurchaseBountyShopCurrencyItemRequest
                {
                    UserID = User.UserID(),
                    ItemID = body.ItemID,
                    DailyRefresh = context.DailyRefresh
                });

                return Ok(resp);
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PurchaseCurrency");
                return new InternalServerError("Failed to purchase item");
            }
        }
    }
}