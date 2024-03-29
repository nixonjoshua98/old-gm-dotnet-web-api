﻿using GMServer.Common;
using GMServer.Models;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.QuestHandlers
{
    public class CompleteMercQuestRequest : IRequest<CompleteMercQuestResponse>
    {
        public string UserID;
        public int QuestID;
        public LocalGameState GameState;
    }

    public class CompleteMercQuestResponse : AbstractResponseWithError
    {
        public int UnlockedMerc;

        public CompleteMercQuestResponse(int merc)
        {
            UnlockedMerc = merc;
        }

        public CompleteMercQuestResponse(string message, int code) : base(message, code)
        {
        }
    }

    public class CompleteMercQuestHandler : IRequestHandler<CompleteMercQuestRequest, CompleteMercQuestResponse>
    {
        private readonly QuestsService _quests;
        private readonly MercsService _mercs;

        public CompleteMercQuestHandler(MercsService mercs, QuestsService quests)
        {
            _quests = quests;
            _mercs = mercs;
        }

        public async Task<CompleteMercQuestResponse> Handle(CompleteMercQuestRequest request, CancellationToken cancellationToken)
        {
            return await HandleRequest(request);
        }

        async Task<(UserMercQuest, UserMerc)> LoadUserDataFromMongo(string userId, MercQuest quest)
        {
            var questProgressTask = _quests.GetMercQuestProgressAsync(userId, quest.ID);
            var userMercTask = _mercs.GetMerc(userId, quest.RewardMercID);

            await Task.WhenAll(questProgressTask, userMercTask);

            return (questProgressTask.Result, userMercTask.Result);
        }

        public async Task<CompleteMercQuestResponse> HandleRequest(CompleteMercQuestRequest request)
        {
            var datafile = _quests.GetDataFile();
            var quest    = datafile.MercQuests.First(x => x.ID == request.QuestID);

            (var userQuest, var userMerc) = await LoadUserDataFromMongo(request.UserID, quest);

            if (userQuest is not null)
                return new("Quest already completed", 400);

            else if (userMerc is not null)
                return new("Merc reward already unlocked", 400);

            else if (!IsQuestComplete(quest, request.GameState))
                return new("Quest requirement not met", 400);

            /* Update Database */
            await _quests.InsertQuestProgress(new UserMercQuest(request.UserID, request.QuestID) { CompletedTime = DateTime.UtcNow });
            await _mercs.InsertMercAsync(new UserMerc(request.UserID, quest.RewardMercID) { UnlockTime = DateTime.UtcNow});

            return new CompleteMercQuestResponse(quest.RewardMercID);
        }

        bool IsQuestComplete(MercQuest quest, LocalGameState localState)
        {
            return quest.ActionType switch
            {
                QuestActionType.StageReached => localState.CurrentStage >= quest.LongValue,
                _ => throw new NotImplementedException()
            };
        }
    }
}
