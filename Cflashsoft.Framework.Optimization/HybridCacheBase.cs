using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents an abstract hybrid cache that uses the in-process .NET MemoryCache and a remote cache in an L1 and L2 cache fashion.
    /// </summary>
    /// <remarks>.NET 9 includes a HybridCache almost identical to this. But this has existed since 2017. A Redis implementation of this hybrid cache exists. Contact RiverFront Solutions for info.</remarks>
    public abstract class HybridCacheBase : IHybridCache
    {
        private NamedSemaphoreSlimLockFactory _namedLocks = new NamedSemaphoreSlimLockFactory();
        private bool _defaultUseMemoryCache = true;
        private bool _defaultUseRemoteCache = false;
        private int _defaultMemoryItemExpirationSeconds = 0;
        private int _defaultRemoteItemExpirationSeconds = 0;
        private bool _defaultMonitorRemoteItems = false;
        private CacheEntryRemovedCallback _memoryCacheEntryRemovedCallback = null;
        private MemoryCache _memoryCache = null;

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        public string Name => _memoryCache.Name;

        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the MemoryCache by default.
        /// </summary>
        public bool DefaultUseMemoryCache => _defaultUseMemoryCache;

        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the remote cache by default.
        /// </summary>
        public bool DefaultUseRemoteCache => _defaultUseRemoteCache;

        /// <summary>
        /// Returns the number of seconds before an item is evicted from the MemoryCache.
        /// </summary>
        public int DefaultMemoryItemExpirationSeconds => _defaultMemoryItemExpirationSeconds;

        /// <summary>
        /// Returns the number of seconds before an item is evicted from the remote cache.
        /// </summary>
        public int DefaultRemoteExpirationSeconds => _defaultRemoteItemExpirationSeconds;

        /// <summary>
        /// Returns true if the MemoryCache monitors for changes in the remote cache.
        /// </summary>
        public bool DefaultMonitorRemoteItems => _defaultMonitorRemoteItems;

        /// <summary>
        /// Returns the underlying MemoryCache instance.
        /// </summary>
        protected MemoryCache MemoryCache => _memoryCache;

        /// <summary>
        /// Initializes a new instance of the HybridCache class.
        /// </summary>
        protected HybridCacheBase(bool defaultUseMemoryCache = true, bool defaultUseRemoteCache = false, int defaultMemoryItemExpirationSeconds = 0, int defaultRemoteItemExpirationSeconds = 0, bool defaultMonitorRemoteItems = false)
            : this("DefaultHybridCache", defaultUseMemoryCache, defaultUseRemoteCache, defaultMemoryItemExpirationSeconds, defaultRemoteItemExpirationSeconds, defaultMonitorRemoteItems)
        { }

        /// <summary>
        /// Initializes a new instance of the HybridCache class.
        /// </summary>
        protected HybridCacheBase(string name, bool defaultUseMemoryCache = true, bool defaultUseRemoteCache = false, int defaultMemoryItemExpirationSeconds = 0, int defaultRemoteItemExpirationSeconds = 0, bool defaultMonitorRemoteItems = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");

            if (!defaultUseMemoryCache && !defaultUseRemoteCache)
                throw new ArgumentException("UseMemoryCache and UseRemoteCache defaults cannot both be false.");

            _defaultUseMemoryCache = defaultUseMemoryCache;
            _defaultUseRemoteCache = defaultUseRemoteCache;
            _defaultMemoryItemExpirationSeconds = defaultMemoryItemExpirationSeconds;
            _defaultRemoteItemExpirationSeconds = defaultRemoteItemExpirationSeconds;
            _defaultMonitorRemoteItems = defaultMonitorRemoteItems;
            _memoryCacheEntryRemovedCallback = new CacheEntryRemovedCallback(OnMemoryCacheEntryRemoved);

            _memoryCache = new MemoryCache(name);
        }


        /// <summary>
        /// Returns an entry from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public virtual T Get<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return GetFromMemoryCache<T>(key, useRemoteCache);
            }
            else
            {
                return GetFromRemoteCache<T>(key);
            }
        }

        /// <summary>
        /// Returns an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public virtual Task<T> GetAsync<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return GetFromMemoryCacheAsync<T>(key, useRemoteCache);
            }
            else
            {
                return GetFromRemoteCacheAsync<T>(key);
            }
        }

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="memoryItemExpirationSeconds">Amount of time before the item is evicted from the MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="remoteItemExpirationSeconds">Amount of time before the item is evicted from the remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="monitorRemoteItem">True if the remote cache will be monitored and items evicted from the MemoryCache on changes. Leave null to use default value set by the cache instance.</param>
        public virtual void Set<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                SetInMemoryCache(key, value, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);
            }
            else
            {
                SetInRemoteCache(key, value, remoteItemExpirationSeconds);
            }
        }

        /// <summary>
        /// Inserts an item into the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="memoryItemExpirationSeconds">Amount of time before the item is evicted from the MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="remoteItemExpirationSeconds">Amount of time before the item is evicted from the remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="monitorRemoteItem">True if the remote cache will be monitored and items evicted from the MemoryCache on changes. Leave null to use default value set by the cache instance.</param>
        public virtual Task SetAsync<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return SetInMemoryCacheAsync(key, value, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);
            }
            else
            {
                return SetInRemoteCacheAsync(key, value, remoteItemExpirationSeconds);
            }
        }


        /// <summary>
        /// Return an item from the cache or insert using the specified function.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="memoryItemExpirationSeconds">Amount of time before the item is evicted from the MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="remoteItemExpirationSeconds">Amount of time before the item is evicted from the remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="monitorRemoteItem">True if the remote cache will be monitored and items evicted from the MemoryCache on changes. Leave null to use default value set by the cache instance.</param>
        /// <param name="getValue">The function that will return an item from the remote store such as a database if the item is not found.</param>
        public virtual T InterlockedGetOrSet<T>(string key, Func<T> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return GetOrSetUsingMemoryCache(key, getValue, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);
            }
            else
            {
                return GetOrSetUsingRemoteCache(key, getValue, remoteItemExpirationSeconds);
            }
        }

        /// <summary>
        /// Return an item from the cache or insert using the specified function.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="memoryItemExpirationSeconds">Amount of time before the item is evicted from the MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="remoteItemExpirationSeconds">Amount of time before the item is evicted from the remote cache. Leave null to use default value set by the cache instance.</param>
        /// <param name="monitorRemoteItem">True if the remote cache will be monitored and items evicted from the MemoryCache on changes. Leave null to use default value set by the cache instance.</param>
        /// <param name="getValue">The function that will return an item from the remote store such as a database if the item is not found.</param>
        public virtual Task<T> InterlockedGetOrSetAsync<T>(string key, Func<Task<T>> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return GetOrSetUsingMemoryCacheAsync(key, getValue, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);
            }
            else
            {
                return GetOrSetUsingRemoteCacheAsync(key, getValue, remoteItemExpirationSeconds);
            }
        }

        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public virtual void Remove(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                RemoveFromMemoryCache(key, useRemoteCache);
            }
            else
            {
                RemoveFromRemoteCache(key);
            }
        }

        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public virtual Task RemoveAsync(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
        {
            AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);

            if (EvaluateUseMemoryCache(useMemoryCache))
            {
                return RemoveFromMemoryCacheAsync(key, useRemoteCache);
            }
            else
            {
                return RemoveFromRemoteCacheAsync(key);
            }
        }

        /// <summary>
        /// Get an item from the MemoryCache.
        /// </summary>
        protected virtual T GetFromMemoryCache<T>(string key, bool? useRemoteCache) where T : class
        {
            T result = (T)_memoryCache.Get(key);

            if (result == null && EvaluateUseRemoteCache(useRemoteCache))
            {
                result = GetFromRemoteCache<T>(key);
            }

            return result;
        }

        /// <summary>
        /// Get an item from the MemoryCache.
        /// </summary>
        protected virtual Task<T> GetFromMemoryCacheAsync<T>(string key, bool? useRemoteCache) where T : class
        {
            T result = GetFromMemoryCache<T>(key, false);

            if (result == null && EvaluateUseRemoteCache(useRemoteCache))
            {
                return GetFromRemoteCacheAsync<T>(key);
            }
            else
            {
                return Task.FromResult(result);
            }
        }

        /// <summary>
        /// Insert an item into the MemoryCache.
        /// </summary>
        protected virtual void SetInMemoryCache<T>(string key, T value, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem) where T : class
        {
            if (value == null)
            {
                RemoveFromMemoryCache(key, useRemoteCache);
            }
            else
            {
                CacheItemPolicy cacheItemPolicy = null;

                if (memoryItemExpirationSeconds.HasValue)
                {
                    if (memoryItemExpirationSeconds.Value > 0)
                    {
                        cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddSeconds(memoryItemExpirationSeconds.Value) };
                    }
                    else
                    {
                        cacheItemPolicy = new CacheItemPolicy();
                    }
                }
                else if (_defaultMemoryItemExpirationSeconds > 0)
                {
                    cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddSeconds(_defaultMemoryItemExpirationSeconds) };
                }
                else
                {
                    cacheItemPolicy = new CacheItemPolicy();
                }

                if (this.CanMonitorRemoteItems)
                {
                    if (monitorRemoteItem.HasValue)
                    {
                        if (monitorRemoteItem.Value)
                        {
                            cacheItemPolicy.ChangeMonitors.Add(GetRemoteItemChangeMonitor(key));
                        }
                    }
                    else if (_defaultMonitorRemoteItems)
                    {
                        cacheItemPolicy.ChangeMonitors.Add(GetRemoteItemChangeMonitor(key));
                    }
                }

                cacheItemPolicy.RemovedCallback = _memoryCacheEntryRemovedCallback;

                _memoryCache.Add(key, value, cacheItemPolicy);

                if (EvaluateUseRemoteCache(useRemoteCache))
                {
                    SetInRemoteCache(key, value, remoteItemExpirationSeconds);
                }
            }
        }

        /// <summary>
        /// Insert an item into the MemoryCache.
        /// </summary>
        protected virtual Task SetInMemoryCacheAsync<T>(string key, T value, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem) where T : class
        {
            if (value == null)
            {
                return RemoveFromMemoryCacheAsync(key, useRemoteCache);
            }
            else
            {
                SetInMemoryCache(key, value, false, memoryItemExpirationSeconds, null, monitorRemoteItem);

                if (EvaluateUseRemoteCache(useRemoteCache))
                {
                    return SetInRemoteCacheAsync(key, value, remoteItemExpirationSeconds);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
        }

        /// <summary>
        /// Return an item from the MemoryCache or insert it using the specified function.
        /// </summary>
        protected virtual T GetOrSetUsingMemoryCache<T>(string key, Func<T> getValue, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem) where T : class
        {
            T result = (T)_memoryCache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    keyLock.Wait();

                    result = (T)_memoryCache.Get(key);

                    if (result == null)
                    {
                        if (EvaluateUseRemoteCache(useRemoteCache))
                        {
                            result = GetOrSetUsingRemoteCache(key, getValue, remoteItemExpirationSeconds);
                        }
                        else
                        {
                            result = getValue();
                        }

                        if (result != null)
                        {
                            SetInMemoryCache(key, result, false, memoryItemExpirationSeconds, null, monitorRemoteItem);
                        }
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
        /// Return an item from the MemoryCache or insert it using the specified function.
        /// </summary>
        protected virtual async Task<T> GetOrSetUsingMemoryCacheAsync<T>(string key, Func<Task<T>> getValue, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem) where T : class
        {
            T result = (T)_memoryCache.Get(key);

            if (result == null)
            {
                var keyLock = _namedLocks.Get(key);

                try
                {
                    await keyLock.WaitAsync();

                    result = (T)_memoryCache.Get(key);

                    if (result == null)
                    {
                        if (EvaluateUseRemoteCache(useRemoteCache))
                        {
                            result = await GetOrSetUsingRemoteCacheAsync(key, getValue, remoteItemExpirationSeconds);
                        }
                        else
                        {
                            result = await getValue();
                        }

                        if (result != null)
                        {
                            SetInMemoryCache(key, result, false, memoryItemExpirationSeconds, null, monitorRemoteItem);
                        }
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
        /// Remove an item from the MemoryCache.
        /// </summary>
        protected virtual void RemoveFromMemoryCache(string key, bool? useRemoteCache)
        {
            _memoryCache.Remove(key);

            if (EvaluateUseRemoteCache(useRemoteCache))
            {
                RemoveFromRemoteCache(key);
            }
        }

        /// <summary>
        /// Remove an item from the MemoryCache.
        /// </summary>
        protected virtual Task RemoveFromMemoryCacheAsync(string key, bool? useRemoteCache)
        {
            RemoveFromMemoryCache(key, false);

            if (EvaluateUseRemoteCache(useRemoteCache))
            {
                return RemoveFromRemoteCacheAsync(key);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Return an item from the remote cache or insert using the specified function.
        /// </summary>
        protected virtual T GetOrSetUsingRemoteCache<T>(string key, Func<T> getValue, int? remoteItemExpirationSeconds) where T : class
        {
            T result = GetFromRemoteCache<T>(key);

            if (result == null)
            {
                using (GetDistributedLock(key))
                {
                    result = GetFromRemoteCache<T>(key);

                    if (result == null)
                    {
                        result = getValue();

                        if (result != null)
                        {
                            SetInRemoteCache(key, result, remoteItemExpirationSeconds);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Return an item from the remote cache or insert using the specified function.
        /// </summary>
        protected virtual async Task<T> GetOrSetUsingRemoteCacheAsync<T>(string key, Func<Task<T>> getValue, int? remoteItemExpirationSeconds) where T : class
        {
            T result = await GetFromRemoteCacheAsync<T>(key);

            if (result == null)
            {
                using (await GetDistributedLockAsync(key))
                {
                    result = await GetFromRemoteCacheAsync<T>(key);

                    if (result == null)
                    {
                        result = await getValue();

                        if (result != null)
                        {
                            await SetInRemoteCacheAsync(key, result, remoteItemExpirationSeconds);
                        }
                    }
                }
            }

            return result;
        }

        private void OnMemoryCacheEntryRemoved(CacheEntryRemovedArguments args)
        {
            _namedLocks.Remove(args.CacheItem.Key);
        }

        /// <summary>
        /// Acquire a distributed lock.
        /// </summary>
        protected abstract IDisposable GetDistributedLock(string key);

        /// <summary>
        /// Acquire a distributed lock.
        /// </summary>
        protected abstract Task<IDisposable> GetDistributedLockAsync(string key);

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        protected abstract T GetFromRemoteCache<T>(string key) where T : class;

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        protected abstract Task<T> GetFromRemoteCacheAsync<T>(string key) where T : class;

        /// <summary>
        /// Insert an item into the remote cache.
        /// </summary>
        protected abstract void SetInRemoteCache<T>(string key, T value, int? remoteItemExpirationSeconds) where T : class;

        /// <summary>
        /// Insert an item into the remote cache.
        /// </summary>
        protected abstract Task SetInRemoteCacheAsync<T>(string key, T value, int? remoteItemExpirationSeconds) where T : class;

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        protected abstract void RemoveFromRemoteCache(string key);

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        protected abstract Task RemoveFromRemoteCacheAsync(string key);

        /// <summary>
        /// Returns true if items will be removed from the MemoryCache when they are no longer in the remote cache.
        /// </summary>
        protected abstract bool CanMonitorRemoteItems { get; }

        /// <summary>
        /// Returns a ChangeMonitor that monitors the remote cache for changes.
        /// </summary>
        protected abstract ChangeMonitor GetRemoteItemChangeMonitor(string key);

        /// <summary>
        /// Throws an exception if the cache operation parameters do not contain expected values.
        /// </summary>
        protected virtual void AssertHybridCacheParameters(string key, bool? useMemoryCache, bool? useRemoteCache)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.");

            if (useMemoryCache.HasValue && !useMemoryCache.Value && useRemoteCache.HasValue && !useRemoteCache.Value)
                throw new ArgumentException("UseMemoryCache and UseRemoteCache cannot both be false.");
        }

        private bool EvaluateUseMemoryCache(bool? useMemoryCache)
        {
            return (useMemoryCache.HasValue && useMemoryCache.Value) || (!useMemoryCache.HasValue && _defaultUseMemoryCache);
        }

        private bool EvaluateUseRemoteCache(bool? useRemoteCache)
        {
            return (useRemoteCache.HasValue && useRemoteCache.Value) || (!useRemoteCache.HasValue && _defaultUseRemoteCache);
        }
    }
}
