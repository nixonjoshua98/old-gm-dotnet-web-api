using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.UserModels.UserModels
{
    /*
     * NB, When adding a new currency we also need to add the currency to the service for updates
     */

    [BsonIgnoreExtraElements]
    public class UserCurrencies
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public int Diamonds { get; set; } = 0;

        public double PrestigePoints { get; set; } = 0;

        public long BountyPoints { get; set; } = 0;

        public long ArmouryPoints { get; set; } = 0;
    }
}