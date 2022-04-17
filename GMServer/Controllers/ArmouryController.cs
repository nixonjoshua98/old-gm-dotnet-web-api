using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.ArmouryHandlers;
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
    public class ArmouryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ArmouryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("Upgrade")]
        [Authorize]
        public async Task<IActionResult> UpgradeItem(UpgradeArmouryItemBody body)
        {
            try
            {
                var resp = await _mediator.Send(new UpgradeArmouryItemRequest()
                {
                    UserID = User.UserID(),
                    ItemID = body.ItemID
                });

                return this.ResponseOrError(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpgradeArmouryItem");
                return new InternalServerError("Failed to upgrade item");
            }
        }
    }
}
