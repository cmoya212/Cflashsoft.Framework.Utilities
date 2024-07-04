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
    /// <remarks>A Redis implementation of this hybrid cache exists. Contact RiverFront Solutions for info.</remarks>
    public class HybridCache : HybridCacheBase
    {
        IHybridCacheRemoteCacheProvider _remoteCache = null;
        IHybridCacheDistributedLockProvider _distributedLockProvider = null;
        IHybridCacheChangeMonitorProvider _changeMonitorProvider = null;

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
        protected override async Task<IDisposable> GetDistributedLockAsync(string key)
        {
            if (_distributedLockProvider != null)
                return await _distributedLockProvider.LockAsync($"HybridCache_{this.Name}_{key}");
            else
                return null;
        }
    }
}
