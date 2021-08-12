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
    public static class MemoryCacheExtensions2
    {
        private static NamedSemaphoreSlimLockFactory _namedLocks = null;
        private static PostEvictionDelegate _removedCallback = null;

        static MemoryCacheExtensions2()
        {
            _namedLocks = new NamedSemaphoreSlimLockFactory();
            _removedCallback = new PostEvictionDelegate(OnRemove);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
        public static object SyncedGetOrSet(this IMemoryCache cache, string key, Func<object> getValue, int expirationSeconds = 0)
        {
            return SyncedGetOrSet(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
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
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
        public static T SyncedGetOrSet<T>(this IMemoryCache cache, string key, Func<T> getValue, int expirationSeconds = 0)
            where T : class
        {
            return SyncedGetOrSet<T>(cache, key, getValue, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
        }


        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
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
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
        public static Task<object> SyncedGetOrSetAsync(this IMemoryCache cache, string key, Func<Task<object>> getValueAsync, int expirationSeconds = 0)
        {
            return SyncedGetOrSetAsync(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
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
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
        public static Task<T> SyncedGetOrSetAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> getValueAsync, int expirationSeconds = 0)
            where T : class
        {
            return SyncedGetOrSetAsync<T>(cache, key, getValueAsync, expirationSeconds > 0 ? DateTime.Now.AddSeconds(expirationSeconds) : DateTimeOffset.MinValue);
        }

        /// <summary>
        /// Return an item from the memory cache or insert it using the provided function.
        /// </summary>
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

        private static void Set(IMemoryCache cache, string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
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
                _namedLocks.Remove((string)key);
        }
    }
}
