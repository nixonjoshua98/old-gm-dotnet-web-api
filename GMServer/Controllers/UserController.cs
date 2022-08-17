using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.Models.RequestModels;
using SRC.Services;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
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