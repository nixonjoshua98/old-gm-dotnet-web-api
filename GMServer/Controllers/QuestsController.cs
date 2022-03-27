using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.QuestHandlers;
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
    public class QuestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly QuestsService _quests;

        public QuestsController(IMediator mediator, QuestsService quests)
        {
            _mediator = mediator;
            _quests = quests;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetQuests([FromServices] RequestContext context)
        {
            try
            {
                var resp = new
                {
                    DateTime = DateTime.UtcNow,
                    CompletedDailyQuests = await _quests.GetCompletedDailyQuestsAsync(User.UserID(), context.DailyRefresh),
                    CompletedMercQuests = await _quests.GetCompletedMercQuestsAsync(User.UserID()),
                    Quests = _quests.GetDataFile()
                };

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetQuests");
                return new InternalServerError("Failed to get quests");
            }
        }

        [HttpPut("Merc")]
        [Authorize]
        public async Task<IActionResult> CompleteMercQuest(CompleteMercQuestBody body)
        {
            try
            {
                var resp = await _mediator.Send(new CompleteMercQuestRequest
                {
                    UserID = User.UserID(),
                    QuestID = body.QuestID,
                    HighestStageReached = body.HighestStageReached
                });

                return Ok(resp);
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompleteMercQuest");
                return new InternalServerError("Failed to complete quest");
            }
        }

        [HttpPut("Daily")]
        [Authorize]
        public async Task<IActionResult> CompleteDailyQuest(CompleteQuestBody body, [FromServices] RequestContext context)
        {
            try
            {
                var resp = await _mediator.Send(new CompleteDailyQuestRequest()
                {
                    UserID = User.UserID(),
                    QuestID = body.QuestID,
                    LocalDailyStats = body.LocalDailyStats,
                    DailyRefresh = context.DailyRefresh
                });

                return Ok(resp);
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompleteDailyQuest");
                return new InternalServerError("Failed to complete quest");
            }
        }
    }
}
