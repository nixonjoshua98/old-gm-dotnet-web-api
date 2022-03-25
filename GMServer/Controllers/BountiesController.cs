using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.BountyHandlers;
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

        [HttpGet("Claim")]
        [Authorize]
        public async Task<IActionResult> ClaimBountyPoints()
        {
            try
            {
                var resp = await _mediator.Send(new ClaimBountyPointRequest()
                {
                    UserID = User.UserID(),
                    DateTime = DateTime.UtcNow
                });

                return Ok(resp);
            } 
            catch (Exception ex)
            {
                Log.Error(ex, "ClaimBountyPoints");
                return new InternalServerError("Failed to claim points");
            }
        }
    }
}
