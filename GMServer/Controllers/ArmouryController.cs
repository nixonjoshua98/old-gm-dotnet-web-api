using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.MediatR.ArmouryHandlers;
using SRC.Models.RequestModels;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
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

        [Authorize]
        [HttpPut("Upgrade")]
        public async Task<IActionResult> UpgradeItem(UpgradeArmouryItemBody body)
        {
            try
            {
                var resp = await _mediator.Send(new UpgradeArmouryItemCommand(UserID: User.UserID(), ItemID: body.ItemID));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpgradeArmouryItem");
                return ServerError.InternalServerError;
            }
        }
    }
}
