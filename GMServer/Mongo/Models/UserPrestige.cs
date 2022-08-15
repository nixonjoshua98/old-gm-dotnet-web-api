using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GMServer.Mongo.Models
{
    public class UserPrestige
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int Stage;
        public DateTime DateTime;
        public double PrestigePointsGained;

        public UserPrestige(string user, int stage, DateTime date, double points)
        {
            UserID = user;
            Stage = stage;
            DateTime = date;
            PrestigePointsGained = points;
        }
    }
}
