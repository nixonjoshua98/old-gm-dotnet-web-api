using GMServer.Common.Encryption;
using GMServer.Common.Types;
using GMServer.Extensions;
using GMServer.MediatR.BountyHandlers;
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

        [Authorize]
        [EncryptedRequestBody]
        [HttpPut("Toggle")]
        public async Task<IActionResult> ToggleActiveBounty(SetActiveBountyBody body)
        {
            try
            {
                var resp = await _mediator.Send(new ToggleActiveBountyCommand(UserID: User.UserID(),
                                                                              BountyID: body.BountyID,
                                                                              IsActive: body.IsActive));

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ToggleActiveBounty");
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