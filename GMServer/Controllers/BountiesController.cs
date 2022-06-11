using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.BountyHandlers;
using GMServer.Models.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using GMServer.Encryption;

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

        [HttpGet("Claim")]
        [Authorize]
        public async Task<IActionResult> ClaimBountyPoints()
        {
            try
            {
                var resp = await _mediator.Send(new ClaimBountyPointRequest
                {
                    UserID = User.UserID()
                });

                return this.ResponseOrError(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ClaimBountyPoints");
                return new InternalServerError("Failed to claim points");
            }
        }

        [Authorize]
        [EncryptedRequestBody]
        [HttpPut("Toggle")]
        public async Task<IActionResult> ToggleActiveBounty(SetActiveBountyBody body)
        {
            try
            {
                var resp = await _mediator.Send(new ToggleActiveBountyRequest
                {
                    UserID = User.UserID(),
                    IsActive = body.IsActive,
                    BountyID = body.BountyID
                });

                return this.ResponseOrError(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ToggleActiveBounty");
                return new InternalServerError();
            }
        }

        [HttpPut("Upgrade")]
        [Authorize]
        public async Task<IActionResult> UpgradeBounty(UpgradeBountyBody body)
        {
            try
            {
                var resp = await _mediator.Send(new UpgradeBountyRequest()
                {
                    UserID = User.UserID(),
                    BountyID = body.BountyID
                });

                return this.ResponseOrError(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpgradeBounty");
                return new InternalServerError("Failed to upgrade bounty");
            }
        }
    }
}