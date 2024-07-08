using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents a hybrid cache that uses the in-process .NET MemoryCache and a remote cache in an L1 and L2 cache fashion.
    /// </summary>
    /// <remarks>.NET 9 includes a HybridCache almost identical to this. But this has existed since 2017. A Redis implementation of this hybrid cache exists. Contact RiverFront Solutions for info.</remarks>
    /// <inheritdoc cref="Cflashsoft.Framework.Optimization.HybridCacheBase"/>
    public class HybridCache : HybridCacheBase
    {
        IHybridCacheRemoteCacheProvider _remoteCache = null;
        IHybridCacheDistributedLockProvider _distributedLockProvider = null;
        IHybridCacheChangeMonitorProvider _changeMonitorProvider = null;

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        public override string Name => base.Name;

        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the MemoryCache by default.
        /// </summary>
        public override bool DefaultUseMemoryCache => base.DefaultUseMemoryCache;

        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the remote cache by default.
        /// </summary>
        public override bool DefaultUseRemoteCache => base.DefaultUseRemoteCache;

        /// <summary>
        /// Returns the number of seconds before an item is evicted from the MemoryCache.
        /// </summary>
        public override int DefaultMemoryItemExpirationSeconds => base.DefaultMemoryItemExpirationSeconds;

        /// <summary>
        /// Returns the number of seconds before an item is evicted from the remote cache.
        /// </summary>
        public override int DefaultRemoteExpirationSeconds => base.DefaultRemoteExpirationSeconds;

        /// <summary>
        /// Returns true if the MemoryCache monitors for changes in the remote cache.
        /// </summary>
        public override bool DefaultMonitorRemoteItems => base.DefaultMonitorRemoteItems;

        /// <summary>
        /// Returns the underlying MemoryCache instance.
        /// </summary>
        protected override MemoryCache MemoryCache => base.MemoryCache;

        /// <summary>
        /// Returns true if items will be removed from the MemoryCache when they are no longer in the remote cache.
        /// </summary>
        protected override bool CanMonitorRemoteItems => _changeMonitorProvider != null;

        /// <summary>
        /// Initializes a new instance of the HybridCache class.
        /// </summary>
        public HybridCache(IHybridCacheRemoteCacheProvider remoteCache, IHybridCacheDistributedLockProvider distributedLockProvider, IHybridCacheChangeMonitorProvider changeMonitorProvider, string name, bool defaultUseMemoryCache = true, bool defaultUseRemoteCache = false, int defaultMemoryItemExpirationSeconds = 0, int defaultRemoteItemExpirationSeconds = 0, bool defaultMonitorRemoteItems = false)
            : base(name, defaultUseMemoryCache, defaultUseRemoteCache, defaultMemoryItemExpirationSeconds, defaultRemoteItemExpirationSeconds, defaultMonitorRemoteItems)
        {
            _remoteCache = remoteCache;
            _distributedLockProvider = distributedLockProvider;
            _changeMonitorProvider = changeMonitorProvider;
        }

        /// <summary>
        /// Returns an entry from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public override T Get<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
            => base.Get<T>(key, useMemoryCache, useRemoteCache);

        /// <summary>
        /// Returns an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public override Task<T> GetAsync<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
            => base.GetAsync<T>(key, useMemoryCache, useRemoteCache);

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
        public override void Set<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null)
            => base.Set<T>(key, value, useMemoryCache, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

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
        public override Task SetAsync<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null)
            => base.SetAsync<T>(key, value, useMemoryCache, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

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
        public override T InterlockedGetOrSet<T>(string key, Func<T> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null)
            => base.InterlockedGetOrSet<T>(key, getValue, useMemoryCache, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

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
        public override Task<T> InterlockedGetOrSetAsync<T>(string key, Func<Task<T>> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null)
            => base.InterlockedGetOrSetAsync<T>(key, getValue, useMemoryCache, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public override void Remove(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
            => base.Remove(key, useMemoryCache, useRemoteCache);

        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        public override Task RemoveAsync(string key, bool? useMemoryCache = null, bool? useRemoteCache = null)
            => base.RemoveAsync(key, useMemoryCache, useRemoteCache);

        /// <summary>
        /// Get an item from the MemoryCache.
        /// </summary>
        protected override T GetFromMemoryCache<T>(string key, bool? useRemoteCache)
            => base.GetFromMemoryCache<T>(key, useRemoteCache);

        /// <summary>
        /// Get an item from the MemoryCache.
        /// </summary>
        protected override Task<T> GetFromMemoryCacheAsync<T>(string key, bool? useRemoteCache)
            => base.GetFromMemoryCacheAsync<T>(key, useRemoteCache);

        /// <summary>
        /// Insert an item into the MemoryCache.
        /// </summary>
        protected override void SetInMemoryCache<T>(string key, T value, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem)
            => base.SetInMemoryCache<T>(key, value, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

        /// <summary>
        /// Return an item from the MemoryCache or insert it using the specified function.
        /// </summary>
        protected override T GetOrSetUsingMemoryCache<T>(string key, Func<T> getValue, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem)
            => base.GetOrSetUsingMemoryCache<T>(key, getValue, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

        /// <summary>
        /// Return an item from the MemoryCache or insert it using the specified function.
        /// </summary>
        protected override Task<T> GetOrSetUsingMemoryCacheAsync<T>(string key, Func<Task<T>> getValue, bool? useRemoteCache, int? memoryItemExpirationSeconds, int? remoteItemExpirationSeconds, bool? monitorRemoteItem)
            => base.GetOrSetUsingMemoryCacheAsync<T>(key, getValue, useRemoteCache, memoryItemExpirationSeconds, remoteItemExpirationSeconds, monitorRemoteItem);

        /// <summary>
        /// Remove an item from the MemoryCache.
        /// </summary>
        protected override void RemoveFromMemoryCache(string key, bool? useRemoteCache)
            => base.RemoveFromMemoryCache(key, useRemoteCache);

        /// <summary>
        /// Remove an item from the MemoryCache.
        /// </summary>
        protected override Task RemoveFromMemoryCacheAsync(string key, bool? useRemoteCache)
            => base.RemoveFromMemoryCacheAsync(key, useRemoteCache);

        /// <summary>
        /// Return an item from the remote cache or insert using the specified function.
        /// </summary>
        protected override T GetOrSetUsingRemoteCache<T>(string key, Func<T> getValue, int? remoteItemExpirationSeconds)
            => base.GetOrSetUsingRemoteCache<T>(key, getValue, remoteItemExpirationSeconds);

        /// <summary>
        /// Return an item from the remote cache or insert using the specified function.
        /// </summary>
        protected override Task<T> GetOrSetUsingRemoteCacheAsync<T>(string key, Func<Task<T>> getValue, int? remoteItemExpirationSeconds)
            => base.GetOrSetUsingRemoteCacheAsync<T>(key, getValue, remoteItemExpirationSeconds);

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        protected override T GetFromRemoteCache<T>(string key)
        {
            return _remoteCache.Get<T>(key);
        }

        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        protected override Task<T> GetFromRemoteCacheAsync<T>(string key)
        {
            return _remoteCache.GetAsync<T>(key);
        }

        /// <summary>
        /// Insert an item into the remote cache.
        /// </summary>
        protected override void SetInRemoteCache<T>(string key, T value, int? remoteItemExpirationSeconds)
        {
            if (value == null)
            {
                RemoveFromRemoteCache(key);
            }
            else
            {
                if (remoteItemExpirationSeconds.HasValue)
                {
                    if (remoteItemExpirationSeconds.Value > 0)
                    {
                        _remoteCache.Set(key, value, remoteItemExpirationSeconds.Value);
                    }
                    else
                    {
                        _remoteCache.Set(key, value);
                    }
                }
                else if (this.DefaultRemoteExpirationSeconds > 0)
                {
                    _remoteCache.Set(key, value, this.DefaultRemoteExpirationSeconds);
                }
                else
                {
                    _remoteCache.Set(key, value);
                }
            }
        }

        /// <summary>
        /// Insert an item into the remote cache.
        /// </summary>
        protected override Task SetInRemoteCacheAsync<T>(string key, T value, int? remoteItemExpirationSeconds)
        {
            if (value == null)
            {
                return RemoveFromRemoteCacheAsync(key);
            }
            else
            {
                if (remoteItemExpirationSeconds.HasValue)
                {
                    if (remoteItemExpirationSeconds.Value > 0)
                    {
                        return _remoteCache.SetAsync(key, value, remoteItemExpirationSeconds.Value);
                    }
                    else
                    {
                        return _remoteCache.SetAsync(key, value);
                    }
                }
                else if (this.DefaultRemoteExpirationSeconds > 0)
                {
                    return _remoteCache.SetAsync(key, value, this.DefaultRemoteExpirationSeconds);
                }
                else
                {
                    return _remoteCache.SetAsync(key, value);
                }
            }
        }

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        protected override void RemoveFromRemoteCache(string key)
        {
            _remoteCache.Remove(key);
        }

        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        protected override Task RemoveFromRemoteCacheAsync(string key)
        {
            return _remoteCache.RemoveAsync(key);
        }

        /// <summary>
        /// Returns a ChangeMonitor that monitors the remote cache for changes.
        /// </summary>
        protected override ChangeMonitor GetRemoteItemChangeMonitor(string key)
        {
            return _changeMonitorProvider.NewChangeMonitor(key);
        }

        /// <summary>
        /// Acquire a distributed lock.
        /// </summary>
        protected override IDisposable GetDistributedLock(string key)
        {
            if (_distributedLockProvider != null)
                return _distributedLockProvider.Lock($"HybridCache_{this.Name}_{key}");
            else
                return null;
        }

        /// <summary>
        /// Acquire a distributed lock.
        /// </summary>
        protected override Task<IDisposable> GetDistributedLockAsync(string key)
        {
            if (_distributedLockProvider != null)
                return _distributedLockProvider.LockAsync($"HybridCache_{this.Name}_{key}");
            else
                return Task.FromResult<IDisposable>(null);
        }

        /// <summary>
        /// Throws an exception if the cache operation parameters do not contain expected values.
        /// </summary>
        protected override void AssertHybridCacheParameters(string key, bool? useMemoryCache, bool? useRemoteCache)
            => base.AssertHybridCacheParameters(key, useMemoryCache, useRemoteCache);
    }
}
