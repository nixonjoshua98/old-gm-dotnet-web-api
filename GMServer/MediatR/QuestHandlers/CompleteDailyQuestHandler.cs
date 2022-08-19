using MediatR;
using SRC.Caching.DataFiles.Models;
using SRC.Common.Enums;
using SRC.Common.Types;
using SRC.Context;
using SRC.DataFiles.Cache;
using SRC.Mongo.Models;
using SRC.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.MediatR.QuestHandlers
{
    public record CompleteDailyQuestCommand(string UserID,
                                            int QuestID,
                                            DailyUserAccountStats LocalDailyStats,
                                            CurrentServerRefresh<IDailyRefresh> DailyRefresh) : IRequest<Result<CompleteDailyQuestResponse>>;

    public record CompleteDailyQuestResponse(int DiamondsRewarded);

    public class CompleteDailyQuestHandler : IRequestHandler<CompleteDailyQuestCommand, Result<CompleteDailyQuestResponse>>
    {
        private readonly QuestsService _quests;
        private readonly CurrenciesService _currencies;
        private readonly IDataFileCache _dataFiles;
        public CompleteDailyQuestHandler(QuestsService quests, CurrenciesService currencies, IDataFileCache dataFiles)
        {
            _quests = quests;
            _currencies = currencies;
            _dataFiles = dataFiles;
        }

        private bool ValidateRequest(CompleteDailyQuestCommand request, UserDailyQuest questProgress, DailyQuest questData, out ServerError error)
        {
            error = default;

            if (!request.DailyRefresh.IsBetween(request.LocalDailyStats.DateTime))
                error = new("Local data is outdated", 400);

            else if (questProgress is not null)
                error = new("Quest already completed", 400);

            else if (!IsQuestCompleted(questData, request.LocalDailyStats))
                error = new("Quest requirements not met", 400);

            return error == default;
        }

        public async Task<Result<CompleteDailyQuestResponse>> Handle(CompleteDailyQuestCommand request, CancellationToken cancellationToken)
        {
            var datafile = _dataFiles.Quests;
            DailyQuest quest = datafile.DailyQuests.First(x => x.ID == request.QuestID);
            UserDailyQuest? userQuest = await _quests.GetDailyQuestProgressAsync(request.UserID, request.QuestID, request.DailyRefresh);

            if (!ValidateRequest(request, userQuest, quest, out var error))
                return error;

            await _quests.InsertQuestProgress(new UserDailyQuest(request.UserID, request.QuestID) { CompletedTime = DateTime.UtcNow });
            await _currencies.IncrementAsync(request.UserID, new() { Gemstones = quest.DiamondsRewarded });

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
