using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.MediatR.BountyHandlers;
using SRC.Models.RequestModels;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BountiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BountiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("Claim")]
        public async Task<IActionResult> ClaimBountyPoints()
        {
            try
            {
                var resp = await _mediator.Send(new ClaimBountyPointRequest(UserID: User.UserID()));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ClaimBountyPoints");
                return ServerError.InternalServerError;
            }
        }

        [HttpPut("Upgrade")]
        [Authorize]
        public async Task<IActionResult> UpgradeBounty(UpgradeBountyBody body)
        {
            try
            {
                var resp = await _mediator.Send(new UpgradeBountyCommand(UserID: User.UserID(), BountyID: body.BountyID));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpgradeBounty");
                return ServerError.InternalServerError;
            }
        }
    }
}