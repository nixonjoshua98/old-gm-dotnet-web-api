using GMServer.Common.Types;
using GMServer.MediatR;
using GMServer.MediatR.Login;
using GMServer.Services;
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
                var resp = await _mediator.Send(new DeviceLoginCommand(DeviceID: deviceId));

                var userData = await _mediator.Send(new GetUserDataRequest
                {
                    UserID = resp.UserID,
                    DailyRefresh = _context.DailyRefresh
                });

                return Ok(new
                {
                    resp.UserID,
                    resp.Token,
                    userData
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeviceLogin");
                return ServerError.InternalServerError;
            }
        }
    }
}
