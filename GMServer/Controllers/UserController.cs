using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.Models.RequestModels;
using GMServer.Services;
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
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AccountStatsService _stats;

        public UserController(IMediator mediator, AccountStatsService stats)
        {
            _mediator = mediator;
            _stats = stats;
        }

        [HttpGet("AccountStats")]
        [Authorize]
        public async Task<IActionResult> GetAccountStats([FromServices] RequestContext context)
        {
            try
            {
                var lifetimeStats = await _stats.GetUserLifetimeStatsAsync(User.UserID());
                var dailyStats = await _stats.GetUserDailyStatsAsync(User.UserID(), context.DailyRefresh);

                return Ok(new { LifetimeStats = lifetimeStats, DailyStats = dailyStats });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateLifetimeStats");
                return new InternalServerError("Failed to update stats");
            }
        }


        [HttpPut("LifetimeStats")]
        [Authorize]
        public async Task<IActionResult> UpdateLifetimeStats(UpdateLifetimeStatsBody body)
        {
            try
            {
                var lifetimeStats = await _stats.UpdateUserLifetimeStatsAsync(User.UserID(), body.Changes);

                return Ok(new { LifetimeStats = lifetimeStats });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateLifetimeStats");
                return new InternalServerError("Failed to update stats");
            }
        }
    }
}