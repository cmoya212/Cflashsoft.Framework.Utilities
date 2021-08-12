using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cflashsoft.Framework.Logging
{
    public class CfFxFileLogProvider : ILoggerProvider
    {
        private readonly CfFxFileLogger _logger = null;

        public CfFxFileLogProvider(IConfiguration config)
        {
            _logger = new CfFxFileLogger(config);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {

        }
    }
}
