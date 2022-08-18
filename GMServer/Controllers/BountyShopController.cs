﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.Common;
using SRC.MediatR.BountyShopHandler;
using SRC.RequestModels.BountyShop;
using SRC.Services.BountyShop;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BountyShopController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBountyShopFactory _shopFactory;

        public BountyShopController(IMediator mediator, IBountyShopFactory shopFactory)
        {
            _mediator = mediator;
            _shopFactory = shopFactory;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {
                var shop = await _shopFactory.GenerateBountyShopAsync(User.UserID());

                return Ok(shop);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetBountyShop");
                return ServerError.InternalServerError;
            }
        }

        [HttpPut("Purchase")]
        [Authorize]
        public async Task<IActionResult> PurchaseItem(PurchaseBountyShopItem body)
        {
            try
            {
                if (body.ItemType == BountyShopItemType.CurrencyItem)
                {
                    var resp = await _mediator.Send(new PurchaseCurrencyItemCommand(UserID: User.UserID(),
                                                                                    ItemID: body.ItemID,
                                                                                    GameDayNumber: Utility.GetGameDayNumber()));
                    return resp.ToResponse();
                }

                else if (body.ItemType == BountyShopItemType.ArmouryItem)
                {
                    var resp = await _mediator.Send(new PurchaseArmouryItemCommand(UserID: User.UserID(),
                                                                                   ItemID: body.ItemID,
                                                                                   GameDayNumber: Utility.GetGameDayNumber()));
                    return resp.ToResponse();
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PurchaseBountyShopItem");
                return ServerError.InternalServerError;
            }
        }
    }
}