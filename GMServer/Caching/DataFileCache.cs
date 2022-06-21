using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GMServer.Cache
{
    internal class DataFileCachedObject
    {
        public DateTime LoadedAt;
        public object Object;

        public DataFileCachedObject(object obj)
        {
            Object = obj;
            LoadedAt = DateTime.UtcNow;
        }
    }

    public interface IDataFileCache
    {
        public T Load<T>(string fp) where T : class;
        public T Load<T>(string fp, Action<T> action) where T : class;
    }


    public class DataFileCache : IDataFileCache
    {
        private readonly Dictionary<string, DataFileCachedObject> _cache;
        private readonly long CacheInterval = 60 * 15;

        public DataFileCache()
        {
            _cache = new Dictionary<string, DataFileCachedObject>();
        }

        public T Load<T>(string fp) where T : class
        {
            return LoadOrCache<T>(fp);
        }

        public T Load<T>(string fp, Action<T> action) where T : class
        {
            T obj = Load<T>(fp);

            action.Invoke(obj);

            return obj;
        }

        private T LoadOrCache<T>(string fp) where T : class
        {
            if (!_cache.TryGetValue(fp, out var cachedObject) || IsOutdated(cachedObject))
            {
                string txt = System.IO.File.ReadAllText(fp);

                T obj = JsonConvert.DeserializeObject<T>(txt);

                cachedObject = _cache[fp] = new(obj);
            }

            return (T)cachedObject.Object;
        }

        private bool IsOutdated(DataFileCachedObject cachedObject)
        {
            long nowTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            long lastLoadTimestamp = new DateTimeOffset(cachedObject.LoadedAt).ToUnixTimeSeconds();

            return (nowTimestamp / CacheInterval) != (lastLoadTimestamp / CacheInterval);
        }
    }
}
