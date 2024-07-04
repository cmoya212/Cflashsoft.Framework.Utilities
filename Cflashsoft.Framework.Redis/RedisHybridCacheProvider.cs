using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Cflashsoft.Framework.Optimization;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cflashsoft.Framework.Redis
{
    /// <summary>
    /// Represents a remote cache store for use with HybridCache.
    /// </summary>
    public class RedisHybridCacheProvider : IHybridCacheRemoteCacheProvider
    {
        private static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        private IConnectionMultiplexer _redis = null;

        /// <summary>
        /// Initializes a new instance of the RedisHybridCacheProvider class.
        /// </summary>
        public RedisHybridCacheProvider(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        public T Get<T>(string key) where T : class
        {
            string text = _redis.GetDatabase().StringGet(key);

            if (text != null)
                return JsonConvert.DeserializeObject<T>(text);
            else
                return null;
        }

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            string text = await _redis.GetDatabase().StringGetAsync(key);

            if (text != null)
                return JsonConvert.DeserializeObject<T>(text);
            else
                return null;
        }

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        public void Remove(string key)
        {
            _redis.GetDatabase().KeyDelete(key);
        }

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        public Task RemoveAsync(string key)
        {
            return _redis.GetDatabase().KeyDeleteAsync(key);
        }

        /// <summary>
        /// Set an item from the remote cache.
        /// </summary>
        public void Set<T>(string key, T value, int? remoteItemExpirationSeconds = null) where T : class
        {
            if (value != null)
                _redis.GetDatabase().StringSet(key, JsonConvert.SerializeObject(value, JsonSerializerSettings), remoteItemExpirationSeconds.HasValue && remoteItemExpirationSeconds.Value > 0 ? TimeSpan.FromSeconds(remoteItemExpirationSeconds.Value) : (TimeSpan?)null);
            else
                Remove(key);
        }

        /// <summary>
        /// Set an item from the remote cache.
        /// </summary>
        public Task SetAsync<T>(string key, T value, int? remoteItemExpirationSeconds = null) where T : class
        {
            if (value != null)
                return _redis.GetDatabase().StringSetAsync(key, JsonConvert.SerializeObject(value, JsonSerializerSettings), remoteItemExpirationSeconds.HasValue && remoteItemExpirationSeconds.Value > 0 ? TimeSpan.FromSeconds(remoteItemExpirationSeconds.Value) : (TimeSpan?)null);
            else
                return RemoveAsync(key);
        }
    }
}
