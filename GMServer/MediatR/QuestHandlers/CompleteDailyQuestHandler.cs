using GMServer.Common;
using GMServer.Context;
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

    public class CompleteDailyQuestResponse : AbstractResponseWithError
    {
        public int DiamondsRewarded;

        public CompleteDailyQuestResponse(int diamondsRewarded)
        {
            DiamondsRewarded = diamondsRewarded;
        }

        public CompleteDailyQuestResponse(string message, int code) : base(message, code)
        {
        }
    }

    public class CompleteDailyQuestHandler : IRequestHandler<CompleteDailyQuestRequest, CompleteDailyQuestResponse>
    {
        private readonly QuestsService _quests;
        private readonly CurrenciesService _currencies;

        public CompleteDailyQuestHandler(QuestsService quests, CurrenciesService currencies)
        {
            _quests = quests;
            _currencies = currencies;
        }

        public async Task<CompleteDailyQuestResponse> Handle(CompleteDailyQuestRequest request, CancellationToken cancellationToken)
        {
            // Last minute check to remove any old data from previous refresh intervals
            if (!request.DailyRefresh.IsBetween(request.LocalDailyStats.DateTime))
                request.LocalDailyStats = new() { DateTime = DateTime.UtcNow };

            var quest = _quests.GetDailyQuest(request.QuestID);
            var userQuest = await _quests.GetDailyQuestProgressAsync(request.UserID, request.QuestID, request.DailyRefresh);

            if (quest is null)
                return new("Quest not found", 400);

            else if (userQuest is not null)
                return new("Quest already completed", 400);

            if (!IsQuestCompleted(quest, request.LocalDailyStats))
                return new("Quest requirements not met", 400);

            await _quests.InsertDailyQuestProgressAsync(new()
            {
                UserID = request.UserID,
                QuestID = request.QuestID,
                CompletedTime = DateTime.UtcNow
            });

            await _currencies.IncrementAsync(request.UserID, new() { Diamonds = quest.DiamondsRewarded });

            return new CompleteDailyQuestResponse(quest.DiamondsRewarded);
        }

        private bool IsQuestCompleted(DailyQuest quest, UserAccountStatsModelBase localStats)
        {
            return quest.ActionType switch
            {
                QuestActionType.Prestige => localStats.TotalPrestiges >= quest.LongValue,
                QuestActionType.EnemiesDefeated => localStats.TotalEnemiesDefeated >= quest.LongValue,
                QuestActionType.BossesDefeated => localStats.TotalBossesDefeated >= quest.LongValue,
                QuestActionType.Taps => localStats.TotalTaps >= quest.LongValue,
                _ => throw new NotImplementedException("Quest type not found")
            };
        }
    }
}
