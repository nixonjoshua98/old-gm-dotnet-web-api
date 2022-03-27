using GMServer.Common;
using GMServer.Context;
using GMServer.Exceptions;
using GMServer.Models.DataFileModels;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.QuestHandlers
{
    public class CompleteDailyQuestRequest : IRequest<CompleteDailyQuestResponse>
    {
        public string UserID;
        public int QuestID;
        public DailyUserAccountStats LocalDailyStats;
        public CurrentServerRefresh<IDailyServerRefresh> DailyRefresh;
    }

    public record CompleteDailyQuestResponse(int DiamondsRewarded);

    public class CompleteDailyQuestHandler : IRequestHandler<CompleteDailyQuestRequest, CompleteDailyQuestResponse>
    {
        private readonly QuestsService _quests;
        private readonly CurrenciesService _currencies;
        private readonly AccountStatsService _accountStats;

        public CompleteDailyQuestHandler(QuestsService quests, CurrenciesService currencies, AccountStatsService accountStats)
        {
            _quests = quests;
            _currencies = currencies;
            _accountStats = accountStats;
        }

        public async Task<CompleteDailyQuestResponse> Handle(CompleteDailyQuestRequest request, CancellationToken cancellationToken)
        {
            // Last minute check to remove any old data from previous refresh intervals
            if (!request.DailyRefresh.IsBetween(request.LocalDailyStats.DateTime))
                request.LocalDailyStats = new() { DateTime = DateTime.UtcNow };

            var quest = _quests.GetDailyQuest(request.QuestID);
            var userQuest = await _quests.GetDailyQuestProgressAsync(request.UserID, request.QuestID, request.DailyRefresh);

            if (quest is null)
                throw new ServerException("Quest not found", 400);

            else if (userQuest is not null)
                throw new ServerException("Quest already completed", 400);

            var dailyStats = await _accountStats.GetUserDailyStatsAsync(request.UserID, request.DailyRefresh);

            if (!IsQuestCompleted(quest, request.LocalDailyStats, dailyStats))
                throw new ServerException("Quest requirements not met", 400);

            await _quests.InsertDailyQuestProgressAsync(new()
            {
                UserID = request.UserID,
                QuestID = request.QuestID,
                CompletedTime = DateTime.UtcNow
            });

            await _currencies.IncrementAsync(request.UserID, new() { Diamonds = quest.DiamondsRewarded });

            return new CompleteDailyQuestResponse(quest.DiamondsRewarded);
        }

        private bool IsQuestCompleted(DailyQuest quest, UserAccountStatsModelBase localStats, DailyUserAccountStats dailyStats)
        {
            return quest.ActionType switch
            {
                QuestActionType.Prestige => dailyStats.TotalPrestiges >= quest.LongValue,
                QuestActionType.EnemiesDefeated => localStats.TotalEnemiesDefeated >= quest.LongValue,
                QuestActionType.BossesDefeated => localStats.TotalBossesDefeated >= quest.LongValue,
                QuestActionType.Taps => localStats.TotalTaps >= quest.LongValue,
                _ => throw new NotImplementedException("Quest type not found")
            };
        }
    }
}
