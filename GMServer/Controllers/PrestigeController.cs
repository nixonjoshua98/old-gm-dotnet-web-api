using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR;
using GMServer.Models.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMServer.Encryption;
using Serilog;
using System;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrestigeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrestigeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut]
        [Authorize]
        [EncryptedRequestBody]
        public async Task<IActionResult> Prestige(PrestigeBody body, [FromServices] RequestContext context)
        {
            try
            {
                var resp = await _mediator.Send(new PrestigeRequest
                {
                    UserID = User.UserID(), 
                    LocalState = body.LocalState
                });

                resp.UserData = await _mediator.Send(new GetUserDataRequest { UserID = User.UserID(), DailyRefresh = context.DailyRefresh });
                resp.DataFiles = await _mediator.Send(new GetDataFileRequest());

                return this.ResponseOrError(resp);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Prestige");
                return new InternalServerError("Failed to prestige");
            }
        }
    }
}
