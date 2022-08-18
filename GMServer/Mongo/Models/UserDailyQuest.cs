using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SRC.Common.Enums;
using System;

namespace SRC.Mongo.Models
{
    public abstract class AbstractUserQuest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int QuestID;

        public DateTime CompletedTime;

        public abstract QuestType QuestType { get; set; }

        public AbstractUserQuest()
        {

        }

        public AbstractUserQuest(string uid, int questId)
        {
            QuestID = questId;
            UserID = uid;
        }
    }


    [BsonIgnoreExtraElements]
    public class UserMercQuest : AbstractUserQuest
    {
        public override QuestType QuestType { get; set; } = QuestType.Merc;

        public UserMercQuest() : base() { }
        public UserMercQuest(string uid, int questId) : base(uid, questId) { }
    }

    [BsonIgnoreExtraElements]
    public class UserDailyQuest : AbstractUserQuest
    {
        public override QuestType QuestType { get; set; } = QuestType.Daily;

        public UserDailyQuest() : base() { }
        public UserDailyQuest(string uid, int questId) : base(uid, questId) { }
    }
}
