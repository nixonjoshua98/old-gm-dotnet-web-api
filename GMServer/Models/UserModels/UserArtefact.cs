using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserArtefact
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int ArtefactID { get; set; }

        public int Level { get; set; } = 0;
    }
}
