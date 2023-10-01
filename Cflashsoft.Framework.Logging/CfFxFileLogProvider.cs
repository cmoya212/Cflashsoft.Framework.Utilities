using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cflashsoft.Framework.Logging
{
    /// <summary>
    /// ILoggerProvider for CfxFileLogger
    /// </summary>
    public class CfFxFileLogProvider : ILoggerProvider
    {
        private readonly CfFxFileLogger _logger = null;

        /// <summary>
        /// Initializing a new instance of this ILoggerProvider.
        /// </summary>
        /// <param name="config">IConfiguration to read settings from.</param>
        public CfFxFileLogProvider(IConfiguration config)
        {
            _logger = new CfFxFileLogger(config);
        }

        /// <summary>
        /// Creates a new ILogger instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The instance of ILogger that was created.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {

        }
    }
}
