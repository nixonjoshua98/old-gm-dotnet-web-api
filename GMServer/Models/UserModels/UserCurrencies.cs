using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GMServer.Models.UserModels
{
    /*
     * NB, When adding a new currency we also need to add the currency to the service for updates
     */

    [BsonIgnoreExtraElements]
    public class UserCurrencies
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int Diamonds = 0;

        public double PrestigePoints = 0;

        public long BountyPoints = 0;

        public long ArmouryPoints = 0;
    }
}