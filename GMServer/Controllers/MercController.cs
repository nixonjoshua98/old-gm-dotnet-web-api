using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.BountyHandlers;
using GMServer.Models.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using GMServer.Encryption;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MercController : ControllerBase
    {
        [HttpGet("Upgrade")]
        [Authorize]
        public async Task<IActionResult> Upgrade()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                Log.Error("Upgrade merc", ex);
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet("Reset")]
        [Authorize]
        public async Task<IActionResult> Reset()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                Log.Error("Reset merc", ex);
                return new InternalServerError(ex.Message);
            }
        }
    }
}