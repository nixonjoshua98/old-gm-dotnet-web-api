using SRC.Mongo.Models;

namespace SRC.Models.RequestModels
{
    public class CompleteDailyQuestBody
    {
        public int QuestID;
        public DailyUserAccountStats LocalDailyStats;
    }

    public class CompleteMercQuestBody
    {
        public int QuestID;
        public SRC.Models.LocalGameState GameState;
    }
}
