using GMServer.Mongo.Models.BaseClasses;

namespace GMServer.Mongo.Models
{
    public class User : MongoDocument
    {
        public string DeviceID;
        public string AccessToken;
    }
}
