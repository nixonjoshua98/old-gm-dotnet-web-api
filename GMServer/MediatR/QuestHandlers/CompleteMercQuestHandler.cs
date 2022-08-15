using GMServer.Caching.DataFiles.Models;
using GMServer.Common;
using GMServer.Common.Types;
using GMServer.Models;
using GMServer.Mongo.Models;
using GMServer.Services;
using MediatR;
using SRC.DataFiles.Cache;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.QuestHandlers
{
    public record CompleteMercQuestRequest(string UserID, int QuestID, LocalGameState GameState) : IRequest<Result<CompleteMercQuestResponse>>;

    public record CompleteMercQuestResponse(int UnlockedMerc);

    public class CompleteMercQuestHandler : IRequestHandler<CompleteMercQuestRequest, Result<CompleteMercQuestResponse>>
    {
        private readonly QuestsService _quests;
        private readonly MercsService _mercs;
        private readonly IDataFileCache _dataFiles;

        public CompleteMercQuestHandler(MercsService mercs, QuestsService quests, IDataFileCache dataFiles)
        {
            _quests = quests;
            _mercs = mercs;
            _dataFiles = dataFiles;
        }

        public async Task<Result<CompleteMercQuestResponse>> Handle(CompleteMercQuestRequest request, CancellationToken cancellationToken)
        {
            var datafile = _dataFiles.Quests;
            var quest = datafile.MercQuests.First(x => x.ID == request.QuestID);

            (var userQuest, var userMerc) = await LoadUserDataFromMongo(request.UserID, quest);

            if (ValidateRequest(request, userQuest, userMerc, quest, out var error))
                return error;

            /* Update Database */
            await _quests.InsertQuestProgress(new UserMercQuest(request.UserID, request.QuestID) { CompletedTime = DateTime.UtcNow });
            await _mercs.InsertMercAsync(new UserMerc(request.UserID, quest.RewardMercID) { UnlockTime = DateTime.UtcNow });

            return new CompleteMercQuestResponse(quest.RewardMercID);
        }

        private async Task<(UserMercQuest, UserMerc)> LoadUserDataFromMongo(string userId, MercQuest quest)
        {
            var questProgressTask = _quests.GetMercQuestProgressAsync(userId, quest.ID);
            var userMercTask = _mercs.GetMerc(userId, quest.RewardMercID);

            await Task.WhenAll(questProgressTask, userMercTask);

            return (questProgressTask.Result, userMercTask.Result);
        }

        private bool ValidateRequest(CompleteMercQuestRequest request, UserMercQuest questProgress, UserMerc userMerc, MercQuest questData, out ServerError error)
        {
            error = default;
            if (questData is null)
                error = new("Quest not found", 400);

            if (questProgress is not null)
                error = new("Quest already completed", 400);

            else if (userMerc is not null)
                error = new("Merc reward already unlocked", 400);

            else if (!IsQuestComplete(questData, request.GameState))
                error = new("Quest requirement not met", 400);

            return error == default;
        }

        private bool IsQuestComplete(MercQuest quest, LocalGameState localState)
        {
            return quest.ActionType switch
            {
                QuestActionType.StageReached => localState.CurrentStage >= quest.LongValue,
                _ => throw new NotImplementedException()
            };
        }
    }
}
