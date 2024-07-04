using Cflashsoft.Framework.Optimization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace Cflashsoft.Framework.Redis
{
    /// <summary>
    /// Represents a ChangeMonitor factory for Redis.
    /// </summary>
    public class RedisChangeMonitorProvider : IHybridCacheChangeMonitorProvider
    {
        IConnectionMultiplexer _redis = null;

        /// <summary>
        /// Initializes a new instance of RedisChangeMonitorProvider class.
        /// </summary>
        public RedisChangeMonitorProvider(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a new HybridCache ChangeMonitor for Redis.
        /// </summary>
        public ChangeMonitor NewChangeMonitor(string key)
        {
            return new RedisChangeMonitor(_redis, key);
        }
    }
}
