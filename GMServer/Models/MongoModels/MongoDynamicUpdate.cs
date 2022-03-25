using GMServer.Attributes;
using GMServer.UserModels.UserModels;
using System.Collections.Generic;
using System.Reflection;

namespace GMServer.Models.MongoModels
{
    public class MongoDynamicUpdate
    {
        public static Dictionary<string, object> CreateDictionary<T>(T obj) where T: UserCurrencies
        {
            Dictionary<string, object> dict = new();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.IsDefined(typeof(MongoIncrementAttribute)))
                {
                    dict.Add(prop.Name, prop.GetValue(obj));
                }
            }
            return dict;
        }
    }
}