using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Extension methods for the .NET ObjectCache and its implementations.
    /// </summary>
    public static class ObjectCacheExtensions
    {
        private static NamedSemaphoreSlimLockFactory _namedLocks = null;
        private static CacheEntryRemovedCallback _evictionCallback = null;

        static ObjectCacheExtensions()
        {
            _namedLocks = new NamedSemaphoreSlimLockFactory();
            _evictionCallback = new CacheEntryRemovedCallback(OnCacheItemEviction);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T InterlockedGetOrSet<T>(this ObjectCache cache, string key, Func<T> getValue, int expirationSeconds = 0, string regionName = null)
            where T : class
        {
            if (expirationSeconds >= 0)
                return InterlockedGetOrSet<T>(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue, regionName);
            else
                return getValue();
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T InterlockedGetOrSet<T>(this ObjectCache cache, string key, Func<T> getValue, DateTimeOffset absoluteExpiration, string regionName = null)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            T result = (T)cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(GetLockName(cache.Name, key));

                try
                {
                    keyLock.Wait();

                    result = (T)cache.Get(key);

                    if (result == null)
                    {
                        result = getValue();

                        if (result == null)
                            throw new InvalidOperationException("The value returned from the getValue function cannot be null.");

                        Set(cache, key, result, absoluteExpiration, regionName);
                    }
                }
                finally
                {
                    keyLock.Release();
                }
            }

            return result;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T? InterlockedGetOrSet<T>(this ObjectCache cache, string key, Func<T?> getValue, int expirationSeconds = 0, string regionName = null)
            where T : struct
        {
            return InterlockedGetOrSet(cache, key, () => GetNullableValue(getValue), expirationSeconds, regionName) as T?;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T? InterlockedGetOrSet<T>(this ObjectCache cache, string key, Func<T?> getValue, DateTimeOffset absoluteExpiration, string regionName = null)
            where T : struct
        {
            return InterlockedGetOrSet(cache, key, () => GetNullableValue(getValue), absoluteExpiration, regionName) as T?;
        }

        private static object GetNullableValue<T>(Func<T?> getValue)
            where T : struct
        {
            T? result = getValue();
            return result.HasValue ? (object)result.Value : (object)null;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static Task<T> InterlockedGetOrSetAsync<T>(this ObjectCache cache, string key, Func<Task<T>> getValueAsync, int expirationSeconds = 0, string regionName = null)
            where T : class
        {
            if (expirationSeconds >= 0)
                return InterlockedGetOrSetAsync<T>(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue, regionName);
            else
                return getValueAsync();
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T> InterlockedGetOrSetAsync<T>(this ObjectCache cache, string key, Func<Task<T>> getValueAsync, DateTimeOffset absoluteExpiration, string regionName = null)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (getValueAsync == null)
                throw new ArgumentNullException(nameof(getValueAsync));

            T result = (T)cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(GetLockName(cache.Name, key));

                try
                {
                    await keyLock.WaitAsync();

                    result = (T)cache.Get(key);

                    if (result == null)
                    {
                        result = await getValueAsync();

                        if (result == null)
                            throw new InvalidOperationException("The value returned from the getValue function cannot be null.");

                        Set(cache, key, result, absoluteExpiration, regionName);
                    }
                }
                finally
                {
                    keyLock.Release();
                }
            }

            return result;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T?> InterlockedGetOrSetAsync<T>(this ObjectCache cache, string key, Func<Task<T?>> getValue, int expirationSeconds = 0, string regionName = null)
            where T : struct
        {
            return (await InterlockedGetOrSetAsync(cache, key, () => GetNullableValueAsync(getValue), expirationSeconds, regionName)) as T?;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T?> InterlockedGetOrSetAsync<T>(this ObjectCache cache, string key, Func<Task<T?>> getValue, DateTimeOffset absoluteExpiration, string regionName = null)
            where T : struct
        {
            return (await InterlockedGetOrSetAsync(cache, key, () => GetNullableValueAsync(getValue), absoluteExpiration, regionName)) as T?;
        }

        private static async Task<object> GetNullableValueAsync<T>(Func<Task<T?>> getValue)
            where T : struct
        {
            T? result = await getValue();
            return result.HasValue ? (object)result.Value : (object)null;
        }

        private static void Set(ObjectCache cache, string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            CacheItemPolicy cacheItemPolicy = null;

            if (absoluteExpiration > DateTimeOffset.MinValue)
            {
                cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = absoluteExpiration };
            }
            else
            {
                cacheItemPolicy = new CacheItemPolicy();
            }

            cacheItemPolicy.RemovedCallback = _evictionCallback;

            cache.Set(key, value, cacheItemPolicy, regionName);
        }

        private static string GetLockName(string sourceName, string key) => $"{sourceName ?? string.Empty}_{key}";

        private static void OnCacheItemEviction(CacheEntryRemovedArguments args) => _namedLocks.Remove(GetLockName(args.Source.Name, args.CacheItem.Key));
    }
}
