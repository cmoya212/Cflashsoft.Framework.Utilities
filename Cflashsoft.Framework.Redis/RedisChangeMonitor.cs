using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Timers;

namespace Cflashsoft.Framework.Redis
{
    /// <summary>
    /// Represents a ChangeMonitor for Redis.
    /// </summary>
    public class RedisChangeMonitor : ChangeMonitor
    {
        #region Static

        //TODO: for now polling every 5 minutes for changes to items. Look into subscribing to some Redis event as an improvement.

        private static event EventHandler Refresh;
        private static Timer Timer = null;
        private static volatile bool IsBusy = false;
        private static object BusySyncLock = new object();

        static RedisChangeMonitor()
        {
            Timer = new Timer();

            Timer.Interval = 300000;
            Timer.Elapsed += OnTimerElapsed;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Run();
        }

        /// <summary>
        /// Force the ChangeMonitor static check manually.
        /// </summary>
        public static void Run()
        {
            lock (BusySyncLock)
            {
                if (IsBusy)
                    return;

                IsBusy = true;
            }

            try
            {
                Refresh?.Invoke(null, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        private bool _disposed = false;
        private string _key = null;
        private string _uniqueId = null;
        IConnectionMultiplexer _redis = null;

        /// <summary>
        /// Gets a value that represents the ChangeMonitor class instance.
        /// </summary>
        public override string UniqueId => _uniqueId;

        /// <summary>
        /// Initializes a new instance of RedisChangeMonitor class.
        /// </summary>
        public RedisChangeMonitor(IConnectionMultiplexer redis, string key)
        {
            _key = key;
            _uniqueId = $"RedisChangeMonitor_{key}";
            _redis = redis;

            Refresh += OnRefresh;

            InitializationComplete();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            //if entry in redis has changed, alert the consuming cache.
            if (!_redis.GetDatabase().KeyExists(_key))
                OnChanged(null);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Refresh -= OnRefresh;
                    _disposed = true;
                }
            }
        }
    }
}
