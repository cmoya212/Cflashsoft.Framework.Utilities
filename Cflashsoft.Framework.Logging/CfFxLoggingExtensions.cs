using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cflashsoft.Framework.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class CfFxLoggingExtensions
    {
        public static ILoggingBuilder AddCfFxFileLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, CfFxFileLogProvider>();
            return builder;
        }
    }
}
