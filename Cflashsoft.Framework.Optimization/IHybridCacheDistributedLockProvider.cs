using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents a factory for creating distributed locks for use with HybridCache.
    /// </summary>
    public interface IHybridCacheDistributedLockProvider
    {
        /// <summary>
        /// Aquire an exclusive distributed lock.
        /// </summary>
        IDisposable Lock(string key);
        /// <summary>
        /// Aquire an exclusive distributed lock.
        /// </summary>
        Task<IDisposable> LockAsync(string key);
    }
}
