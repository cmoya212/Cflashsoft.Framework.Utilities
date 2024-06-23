using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cflashsoft.Framework.Optimization
{
    public class NamedLockWrapper : IDisposable
    {
        public EventHandler Disposed;

        private bool _disposed = false;
        private SemaphoreSlim _semaphore = null;

        public NamedLockWrapper(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disposed(this, EventArgs.Empty);
                }

                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NamedLockWrapper()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
