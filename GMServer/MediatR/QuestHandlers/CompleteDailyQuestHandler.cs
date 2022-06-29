using GMServer.Common;
using GMServer.Context;
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
    public class CompleteDailyQuestRequest : IRequest<CompleteDailyQuestResponse>
    {
        public string UserID;
        public int QuestID;
        public DailyUserAccountStats LocalDailyStats;
        public CurrentServerRefresh<IDailyRefresh> DailyRefresh;
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
            if (!request.DailyRefresh.IsBetween(request.LocalDailyStats.DateTime))
                return new("Local data is outdated", 400);

            /* Fetch datafile */
            var datafile    = _quests.GetDataFile();
            var quest       = datafile.DailyQuests.First(x => x.ID == request.QuestID);

            /* Fetch user data */
            var userQuest = await _quests.GetDailyQuestProgressAsync(request.UserID, request.QuestID, request.DailyRefresh);

            if (userQuest is not null)
                return new("Quest already completed", 400);

            if (!IsQuestCompleted(quest, request.LocalDailyStats))
                return new("Quest requirements not met", 400);

            await _quests.InsertQuestProgress(new UserDailyQuest(request.UserID, request.QuestID) { CompletedTime = DateTime.UtcNow });
            await _currencies.IncrementAsync(request.UserID, new() { Diamonds = quest.DiamondsRewarded });

            return new CompleteDailyQuestResponse(quest.DiamondsRewarded);
        }

        private bool IsQuestCompleted(DailyQuest quest, UserAccountStats localStats)
        {
            return quest.ActionType switch
            {
                QuestActionType.Prestige => localStats.TotalPrestiges >= quest.LongValue,
                QuestActionType.EnemiesDefeated => localStats.TotalEnemiesDefeated >= quest.LongValue,
                QuestActionType.BossesDefeated => localStats.TotalBossesDefeated >= quest.LongValue,
                QuestActionType.Taps => localStats.TotalTaps >= quest.LongValue,
                _ => throw new NotImplementedException()
            };
        }
    }
}
