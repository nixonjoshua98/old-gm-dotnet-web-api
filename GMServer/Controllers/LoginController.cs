using GMServer.Context;
using GMServer.Exceptions;
using GMServer.MediatR;
using GMServer.MediatR.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly RequestContext _context;
        public LoginController(IMediator mediator, RequestContext context)
        {
            _context = context;
            _mediator = mediator;
        }

        [HttpGet("Device")]
        public async Task<IActionResult> DeviceLogin([FromHeader, Required] string deviceId)
        {
            try
            {
                var loginResp = await _mediator.Send(new DeviceLoginRequest { DeviceID = deviceId });

                var dataResp = await _mediator.Send(new GetUserDataRequest
                {
                    UserID = loginResp.UserID,
                    DailyRefresh = _context.DailyRefresh
                });

                return Ok(new
                {
                    UserData = dataResp,
                    loginResp.Token
                });
            }
            catch (ServerException ex)
            {
                Log.Information(ex.Message);
                return new ServerError(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeviceLogin");
                return new InternalServerError("Device login failed");
            }
        }
    }
}
