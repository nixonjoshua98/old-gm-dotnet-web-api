using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserCurrencies
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int Diamonds { get; set; } = 0;

        public int PrestigePoints { get; set; } = 0;

        public int BountyPoints { get; set; } = 0;

        public int ArmouryPoints { get; set; } = 0;
    }
}