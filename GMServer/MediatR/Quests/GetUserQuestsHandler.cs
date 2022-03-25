using GMServer.Context;
using GMServer.Models.DataFileModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class GetUserQuestsRequest: IRequest<GetUserQuestsResponse>
    {
        public string UserID { get; set; }
        public CurrentServerRefresh DailyRefresh;
    }

    public class GetUserQuestsResponse
    {
        public DateTime DateTime;
        public QuestsDataFile Quests { get; set; }
        public List<int> CompletedDailyQuests { get; set; }
        public List<int> CompletedMercQuests { get; set; }
    }

    public class GetUserQuestsHandler : IRequestHandler<GetUserQuestsRequest, GetUserQuestsResponse>
    {
        private readonly QuestsService _quests;

        public GetUserQuestsHandler(QuestsService quests)
        {
            _quests = quests;
        }

        public async Task<GetUserQuestsResponse> Handle(GetUserQuestsRequest request, CancellationToken cancellationToken)
        {
            var resp = new GetUserQuestsResponse()
            {
                DateTime = DateTime.UtcNow,
                Quests = _quests.GetDataFile(),
                CompletedMercQuests = await _quests.GetCompletedMercQuestsAsync(request.UserID),
                CompletedDailyQuests = await _quests.GetCompletedDailyQuestsAsync(request.UserID, request.DailyRefresh)
            };

            return resp;

        }
    }
}
