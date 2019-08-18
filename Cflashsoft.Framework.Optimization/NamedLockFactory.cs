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
    /// Factory for creating named objects for use with thread locks.
    /// </summary>
    public class NamedLockFactory
    {
        /// <summary>
        /// Default factory for creating named objects for use with thread locks.
        /// </summary>
        public static readonly NamedLockFactory Default = new NamedLockFactory();

        private object _syncLock = new object();
        private Dictionary<string, object> _items = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of NamedLockFactory class.
        /// </summary>
        public NamedLockFactory()
        {

        }

        /// <summary>
        /// Get a named lock object.
        /// </summary>
        public object Get(string name)
        {
            if (!_items.TryGetValue(name, out object result))
            {
                lock (_syncLock)
                {
                    if (!_items.TryGetValue(name, out result))
                    {
                        result = new object();
                        _items[name] = result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove a named lock object.
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
