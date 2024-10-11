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
    public static class MemoryCacheExtensions
    {
        private static NamedSemaphoreSlimLockFactory _namedLocks = null;
        private static PostEvictionDelegate _evictionCallback = null;

        static MemoryCacheExtensions()
        {
            _namedLocks = new NamedSemaphoreSlimLockFactory();
            _evictionCallback = new PostEvictionDelegate(OnCacheItemEviction);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T InterlockedGetOrSet<T>(this IMemoryCache cache, string key, Func<T> getValue, int expirationSeconds = 0)
            where T : class
        {
            if (expirationSeconds >= 0)
                return InterlockedGetOrSet<T>(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
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
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T InterlockedGetOrSet<T>(this IMemoryCache cache, string key, Func<T> getValue, DateTimeOffset absoluteExpiration)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

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

                        if (result != null)
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
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T? InterlockedGetOrSet<T>(this IMemoryCache cache, string key, Func<T?> getValue, int expirationSeconds = 0)
            where T : struct
        {
            return InterlockedGetOrSet(cache, key, () => GetNullableValue(getValue), expirationSeconds) as T?;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static T? InterlockedGetOrSet<T>(this IMemoryCache cache, string key, Func<T?> getValue, DateTimeOffset absoluteExpiration)
            where T : struct
        {
            return InterlockedGetOrSet(cache, key, () => GetNullableValue(getValue), absoluteExpiration) as T?;
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
        /// <returns>An object retrieved or set in the cache.</returns>
        public static Task<T> InterlockedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> getValueAsync, int expirationSeconds = 0)
            where T : class
        {
            if (expirationSeconds >= 0)
                return InterlockedGetOrSetAsync<T>(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
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
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T> InterlockedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> getValueAsync, DateTimeOffset absoluteExpiration)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (getValueAsync == null)
                throw new ArgumentNullException(nameof(getValueAsync));

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

                        if (result != null)
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
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="expirationSeconds">Seconds in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T?> InterlockedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T?>> getValue, int expirationSeconds = 0)
            where T : struct
        {
            return (await InterlockedGetOrSetAsync(cache, key, () => GetNullableValueAsync(getValue), expirationSeconds)) as T?;
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function in a thread-safe manner.
        /// </summary>
        /// <param name="cache">The MemoryCache instance.</param>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="getValue">Function to retrieve the value on failure such as from a database. Note: the value returned cannot be null.</param>
        /// <param name="absoluteExpiration">DateTimeOffset in the future that the cache item will expire.</param>
        /// <returns>An object retrieved or set in the cache.</returns>
        public static async Task<T?> InterlockedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T?>> getValue, DateTimeOffset absoluteExpiration)
            where T : struct
        {
            return (await InterlockedGetOrSetAsync(cache, key, () => GetNullableValueAsync(getValue), absoluteExpiration)) as T?;
        }

        private static async Task<object> GetNullableValueAsync<T>(Func<Task<T?>> getValue)
            where T : struct
        {
            T? result = await getValue();
            return result.HasValue ? (object)result.Value : (object)null;
        }

        private static void Set(IMemoryCache cache, string key, object value, DateTimeOffset absoluteExpiration)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

            if (absoluteExpiration > DateTimeOffset.MinValue)
                options.SetAbsoluteExpiration(absoluteExpiration);

            options.RegisterPostEvictionCallback(_evictionCallback);

            cache.Set(key, value, options);
        }

        private static void OnCacheItemEviction(object key, object value, EvictionReason reason, object state)
        {
            string keyString = key as string;

            if (keyString != null)
                _namedLocks.Remove(keyString);
        }
    }
}
