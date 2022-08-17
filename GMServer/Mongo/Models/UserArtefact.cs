using MongoDB.Bson.Serialization.Attributes;

namespace SRC.Mongo.Models
{
    [BsonIgnoreExtraElements]
    public class UserArtefact
    {
        public string UserID;

        public int ArtefactID;

        public int Level = 0;
    }
}
