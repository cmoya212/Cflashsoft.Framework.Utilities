using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace Cflashsoft.Framework.Optimization
{
    /// <summary>
    /// Represents a factory that creates ChangeMonitors for use with HybridCache.
    /// </summary>
    public interface IHybridCacheChangeMonitorProvider
    {
        /// <summary>
        /// Returns a new HybridCache ChangeMonitor.
        /// </summary>
        ChangeMonitor NewChangeMonitor(string key);
    }
}
