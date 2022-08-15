using System.Collections.Generic;

namespace GMServer.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue) where TKey : notnull
        {
            return source.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
