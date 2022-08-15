using GMServer.Common.Encryption;
using GMServer.Common.Types;
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
        [EncryptedResponseBody]
        public async Task<IActionResult> Prestige(PrestigeBody body, [FromServices] RequestContext context)
        {
            try
            {
                var resp = await _mediator.Send(new PrestigeRequest(User.UserID(), body.LocalState));

                var userData = await _mediator.Send(new GetUserDataRequest { UserID = User.UserID(), DailyRefresh = context.DailyRefresh });
                var dataFiles = await _mediator.Send(new GetDataFilesCommand());

                return Ok(new
                {
                    userData,
                    dataFiles
                });
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Prestige");
                return ServerError.InternalServerError;
            }
        }
    }
}
