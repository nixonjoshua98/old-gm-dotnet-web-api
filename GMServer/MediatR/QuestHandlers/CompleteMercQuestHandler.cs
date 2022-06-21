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
            var quest = _quests.GetMercQuest(request.QuestID);
            var questProgress = await _quests.GetMercQuestProgressAsync(request.UserID, request.QuestID);

            if (questProgress is not null)
                return new("Quest already completed", 400);

            else if (quest.RequiredStage > request.HighestStageReached)
                return new("Requirements not met", 400);

            var userMerc = await _mercs.GetMerc(request.UserID, quest.RewardMercID);

            if (userMerc is not null)
                return new("Merc reward already unlocked", 400);

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
