using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Extension methods for the .NET IMemoryCache.
    /// </summary>
    public static class IMemoryCacheExtensions
    {
        private static NamedSemaphoreSlimLockFactory _namedLocks = null;
        private static PostEvictionDelegate _removedCallback = null;

        static IMemoryCacheExtensions()
        {
            _namedLocks = new NamedSemaphoreSlimLockFactory();
            _removedCallback = new PostEvictionDelegate(OnRemove);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static object SyncedGetOrSet(this IMemoryCache cache, string key, Func<object> getValue, int expirationSeconds = 0)
        {
            if (expirationSeconds >= 0)
                return SyncedGetOrSet(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
            else
                return getValue();
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static object SyncedGetOrSet(this IMemoryCache cache, string key, Func<object> getValue, DateTimeOffset absoluteExpiration)
        {
            object result = cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    keyLock.Wait();

                    result = cache.Get(key);

                    if (result == null)
                    {
                        result = getValue();

                        Set(cache, key, result, absoluteExpiration);
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
        /// <param name="getValue">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T SyncedGetOrSet<T>(this IMemoryCache cache, string key, Func<T> getValue, int expirationSeconds = 0)
            where T : class
        {
            if (expirationSeconds >= 0)
                return SyncedGetOrSet<T>(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
            else
                return getValue();
        }


        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T SyncedGetOrSet<T>(this IMemoryCache cache, string key, Func<T> getValue, DateTimeOffset absoluteExpiration)
            where T : class
        {
            T result = (T)cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    keyLock.Wait();

                    result = (T)cache.Get(key);

                    if (result == null)
                    {
                        result = getValue();

                        Set(cache, key, result, absoluteExpiration);
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
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static Task<object> SyncedGetOrSetAsync(this IMemoryCache cache, string key, Func<Task<object>> getValueAsync, int expirationSeconds = 0)
        {
            if (expirationSeconds >= 0)
                return SyncedGetOrSetAsync(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
            else
                return getValueAsync();
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<object> SyncedGetOrSetAsync(this IMemoryCache cache, string key, Func<Task<object>> getValueAsync, DateTimeOffset absoluteExpiration)
        {
            object result = cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    await keyLock.WaitAsync();

                    result = cache.Get(key);

                    if (result == null)
                    {
                        result = await getValueAsync();

                        Set(cache, key, result, absoluteExpiration);
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
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static Task<T> SyncedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> getValueAsync, int expirationSeconds = 0)
            where T : class
        {
            if (expirationSeconds >= 0)
                return SyncedGetOrSetAsync<T>(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
            else
                return getValueAsync();
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValueAsync">Function to retrieve the value on failure such as from a database.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <param name="regionName">The name of a region in a cache. The default is null.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T> SyncedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> getValueAsync, DateTimeOffset absoluteExpiration)
            where T : class
        {
            T result = (T)cache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    await keyLock.WaitAsync();

                    result = (T)cache.Get(key);

                    if (result == null)
                    {
                        result = await getValueAsync();

                        Set(cache, key, result, absoluteExpiration);
                    }
                }
                finally
                {
                    keyLock.Release();
                }
            }

            return result;
        }

        private static void Set(IMemoryCache cache, string key, object value, DateTimeOffset absoluteExpiration)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

            if (absoluteExpiration > DateTimeOffset.MinValue)
                options.SetAbsoluteExpiration(absoluteExpiration);

            options.RegisterPostEvictionCallback(_removedCallback);

            cache.Set(key, value, options);
        }

        private static void OnRemove(object key, object value, EvictionReason reason, object state)
        {
            string keyString = key as string;

            if (keyString != null)
                _namedLocks.Remove(keyString);
        }
    }
}
