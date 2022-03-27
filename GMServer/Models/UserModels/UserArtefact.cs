using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserArtefact
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int ArtefactID;

        public int Level = 0;
    }
}
