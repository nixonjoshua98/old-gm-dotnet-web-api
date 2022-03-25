using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.UserModels.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserDailyQuest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }
        public int QuestID { get; set; }
        public DateTime CompletedTime { get; set; }
    }
}
