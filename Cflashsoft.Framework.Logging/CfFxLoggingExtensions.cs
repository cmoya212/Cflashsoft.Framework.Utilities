using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cflashsoft.Framework.Logging;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Dependency injection extension for CfxFileLogger
    /// </summary>
    public static class CfFxLoggingExtensions
    {
        /// <summary>
        /// Register CfxFileLogger as an ILoggerProvider singleton.
        /// </summary>
        /// <param name="builder">The ILoggingBuilder object.</param>
        /// <returns>Method chained ILoggingBuilder.</returns>
        public static ILoggingBuilder AddCfFxFileLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, CfFxFileLogProvider>();
            return builder;
        }
    }
}
