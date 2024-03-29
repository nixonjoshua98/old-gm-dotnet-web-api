﻿using GMServer.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GMServer.Models.UserModels
{
    [BsonIgnoreExtraElements]
    public class UserMerc
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID;

        public int MercID;

        public DateTime UnlockTime;

        public UserMerc()
        {

        }

        public UserMerc(string uid, int mercId)
        {
            UserID = uid;
            MercID = mercId;
        }
    }
}