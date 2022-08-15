using GMServer.Common.Encryption;
using GMServer.Common.Types;
using GMServer.Extensions;
using GMServer.MediatR.QuestHandlers;
using GMServer.Models.RequestModels;
using GMServer.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.DataFiles.Cache;
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
        private readonly IDataFileCache _dataFiles;

        public QuestsController(IMediator mediator, QuestsService quests, IDataFileCache dataFiles)
        {
            _mediator = mediator;
            _quests = quests;
            _dataFiles = dataFiles;
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
                    Quests = _dataFiles.Quests
                };

                return Ok(resp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetQuests");
                return ServerError.InternalServerError;
            }
        }

        [Authorize]
        [EncryptedRequestBody]
        [HttpPut("Merc")]
        public async Task<IActionResult> CompleteMercQuest(CompleteMercQuestBody body)
        {
            try
            {
                var resp = await _mediator.Send(new CompleteMercQuestRequest(UserID: User.UserID(),
                                                                             QuestID: body.QuestID,
                                                                             GameState: body.GameState));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompleteMercQuest");
                return ServerError.InternalServerError;
            }
        }

        [HttpPut("Daily")]
        [Authorize]
        public async Task<IActionResult> CompleteDailyQuest(CompleteDailyQuestBody body, [FromServices] RequestContext context)
        {
            try
            {
                var resp = await _mediator.Send(new CompleteDailyQuestCommand(UserID: User.UserID(),
                                                                              QuestID: body.QuestID,
                                                                              LocalDailyStats: body.LocalDailyStats,
                                                                              DailyRefresh: context.DailyRefresh));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompleteDailyQuest");
                return ServerError.InternalServerError;
            }
        }
    }
}
