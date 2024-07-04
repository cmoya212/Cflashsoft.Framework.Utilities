using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents a remote cache store for use with HybridCache.
    /// </summary>
    public interface IHybridCacheRemoteCacheProvider
    {
        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        T Get<T>(string key) where T : class;
        /// <summary>
        /// Get an item from the remote cache.
        /// </summary>
        Task<T> GetAsync<T>(string key) where T : class;
        /// <summary>
        /// Set an item from the remote cache.
        /// </summary>
        void Set<T>(string key, T value, int? remoteItemExpirationSeconds = null) where T : class;
        /// <summary>
        /// Set an item from the remote cache.
        /// </summary>
        Task SetAsync<T>(string key, T value, int? remoteItemExpirationSeconds = null) where T : class;
        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        void Remove(string key);
        /// <summary>
        /// Remove an item from the remote cache.
        /// </summary>
        Task RemoveAsync(string key);
    }
}
