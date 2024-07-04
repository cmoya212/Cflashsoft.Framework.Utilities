using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cflashsoft.Framework.Optimization;
using Medallion.Threading;

namespace Cflashsoft.Framework.Redis
{
    /// <summary>
    /// Represents a DistributedLock factory for HybridCache.
    /// </summary>
    public class RedisHybridCacheDistributedLockProvider : IHybridCacheDistributedLockProvider
    {
        private IDistributedLockProvider _distributedLockProvider = null;

        /// <summary>
        /// Initializes a new instance of RedisHybridCacheDistributedLockProvider class.
        /// </summary>
        public RedisHybridCacheDistributedLockProvider(IDistributedLockProvider distributedLockProvider)
        {
            _distributedLockProvider = distributedLockProvider;
        }

        /// <summary>
        /// Aquire an exclusive distributed lock.
        /// </summary>
        public IDisposable Lock(string key) => _distributedLockProvider.AcquireLock(key);

        /// <summary>
        /// Aquire an exclusive distributed lock.
        /// </summary>
        public async Task<IDisposable> LockAsync(string key) => await _distributedLockProvider.AcquireLockAsync(key);
    }
}
