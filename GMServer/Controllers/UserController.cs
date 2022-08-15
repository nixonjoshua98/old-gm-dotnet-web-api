using GMServer.Common.Types;
using GMServer.Extensions;
using GMServer.Models.RequestModels;
using GMServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AccountStatsService _stats;

        public UserController(AccountStatsService stats)
        {
            _stats = stats;
        }

        [HttpPut("LifetimeStats")]
        [Authorize]
        public async Task<IActionResult> UpdateLifetimeStats(UpdateLifetimeStatsBody body)
        {
            try
            {
                var lifetimeStats = await _stats.UpdateLifetimeStatsWithLocalChangesAsync(User.UserID(), body.Changes);

                return Ok(new { LifetimeStats = lifetimeStats });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateLifetimeStats");
                return ServerError.InternalServerError;
            }
        }
    }
}