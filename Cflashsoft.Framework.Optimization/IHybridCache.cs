using System;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents an abstract hybrid cache that uses the in-process .NET MemoryCache and a remote cache in an L1 and L2 cache fashion.
    /// </summary>
    /// <remarks>A Redis implementation of this hybrid cache exists. Contact RiverFront Solutions for info.</remarks>
    public interface IHybridCache
    {
        /// <summary>
        /// Returns the number of seconds before an item is evicted from the MemoryCache.
        /// </summary>
        int DefaultMemoryItemExpirationSeconds { get; }
        /// <summary>
        /// Returns true if the MemoryCache monitors for changes in the remote cache.
        /// </summary>
        bool DefaultMonitorRemoteItems { get; }
        /// <summary>
        /// Returns the number of seconds before an item is evicted from the remote cache.
        /// </summary>
        int DefaultRemoteExpirationSeconds { get; }
        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the MemoryCache by default.
        /// </summary>
        bool DefaultUseMemoryCache { get; }
        /// <summary>
        /// Returns true if the the hybrid cache will store an item in the remote cache by default.
        /// </summary>
        bool DefaultUseRemoteCache { get; }
        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns an entry from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        T Get<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null) where T : class;
        /// <summary>
        /// Returns an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        Task<T> GetAsync<T>(string key, bool? useMemoryCache = null, bool? useRemoteCache = null) where T : class;
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
        T InterlockedGetOrSet<T>(string key, Func<T> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class;
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
        Task<T> InterlockedGetOrSetAsync<T>(string key, Func<Task<T>> getValue, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class;
        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        void Remove(string key, bool? useMemoryCache = null, bool? useRemoteCache = null);
        /// <summary>
        /// Remove an item from the cache.
        /// </summary>
        /// <param name="key">The unique key of the item in the cache.</param>
        /// <param name="useMemoryCache">Use MemoryCache. Leave null to use default value set by the cache instance.</param>
        /// <param name="useRemoteCache">Use remote cache. Leave null to use default value set by the cache instance.</param>
        Task RemoveAsync(string key, bool? useMemoryCache = null, bool? useRemoteCache = null);
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
        void Set<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class;
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
        Task SetAsync<T>(string key, T value, bool? useMemoryCache = null, bool? useRemoteCache = null, int? memoryItemExpirationSeconds = null, int? remoteItemExpirationSeconds = null, bool? monitorRemoteItem = null) where T : class;
    }
}