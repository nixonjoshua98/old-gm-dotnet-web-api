using GMServer.Models.UserModels;

namespace GMServer.Models.RequestModels
{
    public class CompleteDailyQuestBody
    {
        public int QuestID;
        public DailyUserAccountStats LocalDailyStats;
    }

    public class CompleteMercQuestBody
    {
        public int QuestID;
        public GMServer.Models.LocalGameState GameState;
    }
}
