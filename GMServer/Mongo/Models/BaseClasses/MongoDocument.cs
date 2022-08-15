using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Mongo.Models.BaseClasses
{
    public interface IMongoDocument
    {
        string? ID { get; }
    }

    public abstract class MongoDocument : IMongoDocument
    {
        [BsonId]
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; }
    }
}
