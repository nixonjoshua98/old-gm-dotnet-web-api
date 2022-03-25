using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromServices] RequestContext context)
        {
            try
            {
                var userdata = await _mediator.Send(new GetUserDataRequest
                {
                    UserID = User.UserID()
                });

                var quests = await _mediator.Send(new GetUserQuestsRequest()
                {
                    UserID = User.UserID(),
                    DailyRefresh = context.DailyRefresh
                });

                return Ok(new
                {
                    userdata.Artefacts,
                    userdata.ArmouryItems,
                    userdata.Currencies,
                    userdata.UnlockedMercs,
                    userdata.Bounties,
                    Quests = quests,
                });
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }
    }
}