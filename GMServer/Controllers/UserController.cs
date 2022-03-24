using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.UserData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace GMServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _mediator.Send(new GetUserDataRequest()
                {
                    UserID = User.UserID()
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }
    }
}