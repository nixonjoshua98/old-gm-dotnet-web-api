using GMServer.MediatR.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using Serilog;
using GMServer.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoginController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> DeviceLogin([FromHeader, Required] string deviceId)
        {
            try
            {
                var response = await _mediator.Send(new DeviceLoginRequest()
                {
                    DeviceID = deviceId
                });

                return Ok(response);
            }
            catch (HandledException ex)
            {
                Log.Information(ex, "DeviceLogin");
                return new ServerError(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeviceLogin");
                return new InternalServerError(ex.Message);
            }
        }
    }
}
