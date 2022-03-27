using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR;
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
    public class PrestigeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrestigeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Prestige(PrestigeBody body, [FromServices] RequestContext context)
        {
            try
            {
                await _mediator.Send(new PrestigeRequest { UserID = User.UserID(), PrestigeStage = body.PrestigeStage });

                var userData = await _mediator.Send(new GetUserDataRequest { UserID = User.UserID(), DailyRefresh = context.DailyRefresh });
                var datafiles = await _mediator.Send(new GetDataFileRequest());

                return Ok(new
                {
                    DataFiles = datafiles,
                    UserData = userData
                });
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Prestige");
                return new InternalServerError("Failed to prestige");
            }
        }
    }
}
