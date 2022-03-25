using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserMercQuest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int QuestID { get; set; }
    }
}
