using GMServer.Exceptions;
using GMServer.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.QuestHandlers
{
    public class CompleteMercQuestRequest : IRequest<CompleteMercQuestResponse>
    {
        public string UserID;
        public int QuestID;
        public int HighestStageReached;
    }

    public record CompleteMercQuestResponse(int UnlockedMerc);

    public class CompleteMercQuestHandler : IRequestHandler<CompleteMercQuestRequest, CompleteMercQuestResponse>
    {
        private readonly QuestsService _quests;
        private readonly MercService _mercs;

        public CompleteMercQuestHandler(MercService mercs, QuestsService quests)
        {
            _quests = quests;
            _mercs = mercs;
        }

        public async Task<CompleteMercQuestResponse> Handle(CompleteMercQuestRequest request, CancellationToken cancellationToken)
        {
            var quest = _quests.GetMercQuest(request.UserID, request.QuestID);
            var questProgress = await _quests.GetMercQuestProgressAsync(request.UserID, request.QuestID);

            if (questProgress is not null)
                throw new ServerException("Quest already completed", 400);

            else if (quest.RequiredStage > request.HighestStageReached)
                throw new ServerException("Requirements not met", 400);

            var userMerc = await _mercs.GetMerc(request.UserID, quest.RewardMercID);

            if (userMerc is not null)
                throw new ServerException("Merc reward already unlocked", 400);

            await _quests.InsertMercQuestProgressAsync(new()
            {
                UserID = request.UserID,
                QuestID = request.QuestID
            });

            await _mercs.InsertMercAsync(new()
            {
                UserID = request.UserID,
                MercID = quest.RewardMercID
            });

            return new CompleteMercQuestResponse(quest.RewardMercID);
        }
    }
}
