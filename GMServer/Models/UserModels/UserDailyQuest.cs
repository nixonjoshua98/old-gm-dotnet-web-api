using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserDailyQuest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;
        public int QuestID;
        public DateTime CompletedTime;
    }
}
