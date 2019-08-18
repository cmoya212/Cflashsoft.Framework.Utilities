using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Factory for creating named semaphores for use with thread locks.
    /// </summary>
    public class NamedSemaphoreSlimLockFactory
    {
        /// <summary>
        /// Default factory for creating named semaphores for use with thread locks.
        /// </summary>
        public static readonly NamedSemaphoreSlimLockFactory Default = new NamedSemaphoreSlimLockFactory();

        private object _syncLock = new object();
        private Dictionary<string, SemaphoreSlim> _items = new Dictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Initializes a new instance of NamedSemaphoreSlimLockFactory class.
        /// </summary>
        public NamedSemaphoreSlimLockFactory()
        {

        }

        /// <summary>
        /// Get a named semaphore.
        /// </summary>
        public SemaphoreSlim Get(string name, int initialCount = 1, int maxCount = 1)
        {
            if (!_items.TryGetValue(name, out SemaphoreSlim result))
            {
                lock (_syncLock)
                {
                    if (!_items.TryGetValue(name, out result))
                    {
                        result = new SemaphoreSlim(initialCount, maxCount);
                        _items[name] = result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove a named semaphore.
        /// </summary>
        public void Remove(string name)
        {
            lock (_syncLock)
            {
                _items.Remove(name);
            }
        }
    }
}
