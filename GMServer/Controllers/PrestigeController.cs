using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Encryption;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.MediatR;
using SRC.Models.RequestModels;
using SRC.Services;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
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
