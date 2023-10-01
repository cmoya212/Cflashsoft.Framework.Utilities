using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Cflashsoft.Framework.Logging
{
    public delegate void LogAction(DateTime now, LogLevel logLevel, string message, Exception exception);

    /// <summary>
    /// A thread-safe ILogger implementation that writes to log files on disk and rolls over daily and for configured sized limitation.
    /// </summary>
    public class CfFxFileLogger : ILogger
    {
        private readonly object _logFileSyncLock = new object();
        private readonly string _logFilePath = null;
        private readonly bool _dailyRollover = true;
        private readonly int _maxLogFileSize = 536870912; //512mb

        private LogAction _writeFinishedAction = null;
        private LogAction _writeFailureAction = null;

        private int _currentFileNameSuffix = 0;
        private string _currentDateString = null;

        /// <summary>
        /// Initialize a new instance of CfFxFileLogger.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        public CfFxFileLogger(CfFxFileLoggerOptions options)
        {
            string logFilePrefix = options.LogFilePrefix;

            if (string.IsNullOrWhiteSpace(logFilePrefix))
                logFilePrefix = "CfFx";

            string logFilePath = options.LogFilePath;

            if (!string.IsNullOrWhiteSpace(logFilePath))
                _logFilePath = Path.Combine(logFilePath, logFilePrefix);
            else
                _logFilePath = Path.Combine(Path.GetTempPath(), logFilePrefix);

            if (options.DailyRollover.HasValue)
                _dailyRollover = options.DailyRollover.Value;

            if (options.MaxLogFileSize.HasValue)
                _maxLogFileSize = (options.MaxLogFileSize.Value * 1024) * 1024;

            if (options.WriteFinishedAction != null)
                _writeFinishedAction = options.WriteFinishedAction;

            if (options.WriteFailureAction != null)
                _writeFailureAction = options.WriteFailureAction;
        }

        /// <summary>
        /// Initialize a new instance of CfFxFileLogger.
        /// </summary>
        /// <param name="config">Configuration options.</param>
        public CfFxFileLogger(IConfiguration config)
        {
            var cfFxLogConfig = config.GetSection("Logging").GetSection("CfFxLog");

            string logFilePrefix = cfFxLogConfig["LogFilePrefix"];

            if (string.IsNullOrWhiteSpace(logFilePrefix))
                logFilePrefix = "CfFx";

            string logFilePath = cfFxLogConfig["LogFilePath"];

            if (!string.IsNullOrWhiteSpace(logFilePath))
                _logFilePath = Path.Combine(logFilePath, logFilePrefix);
            else
                _logFilePath = Path.Combine(Path.GetTempPath(), logFilePrefix);

            string dailyRollover = cfFxLogConfig["DailyRollover"];

            if (!string.IsNullOrWhiteSpace(dailyRollover))
                _dailyRollover = bool.Parse(dailyRollover);

            string maxLogFileSize = cfFxLogConfig["MaxLogFileSize"];

            if (!string.IsNullOrWhiteSpace(maxLogFileSize))
                _maxLogFileSize = (int.Parse(maxLogFileSize) * 1024) * 1024;
        }

        /// <summary>
        /// In implementations that support it, begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        /// <remarks>NOTE: CfxFileLogger does not implement this functionality.</remarks>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Checks if the given logLevel is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>true if enabled; false otherwise.</returns>
        /// <remarks>NOTE: CfxFileLogger does not implement this functionality. The result will always be true.</remarks>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can also be an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a String message of the state and exception.</param>
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string message = formatter(state, exception);

            try
            {
                WriteEntry(now, logLevel, message, exception);
            }
            catch (Exception ex)
            {
                if (_writeFailureAction == null)
                {
                    throw;
                }
                else
                {
                    Exception failureException = null;

                    if (exception != null)
                        failureException = new AggregateException(ex, exception);
                    else
                        failureException = ex;

                    _writeFailureAction(now.DateTime, LogLevel.Critical, $"Error writing to log file. {logLevel.ToString()}: {message}", failureException);
                }
            }

            if (_writeFinishedAction != null)
            {
                _writeFinishedAction(now.DateTime, logLevel, message, exception);
            }
        }

        private void WriteEntry(DateTimeOffset now, LogLevel logLevel, string message, Exception exception)
        {
            string dateString = null;

            if (_dailyRollover)
            {
                dateString = now.ToString("yyyy-MM-dd");

                if (_currentDateString != dateString)
                {
                    _currentDateString = dateString;
                    _currentFileNameSuffix = 0;
                }
            }
            else
            {
                dateString = string.Empty;
            }

            lock (_logFileSyncLock)
            {
                while (_currentFileNameSuffix < 99999)
                {
                    using (var writer = new StreamWriter($"{_logFilePath}_{(dateString != string.Empty ? $"{dateString}_" : string.Empty)}{_currentFileNameSuffix.ToString("D5")}.log", true))
                    {
                        if (writer.BaseStream.Length < _maxLogFileSize)
                        {
                            writer.Write('[');
                            //writer.Write(now.ToString("yyyy-MM-dd HH:mm:ss+00:00"));
                            writer.Write(now.ToString("yyyy-MM-dd HH:mm:ss"));
                            writer.Write(']');
                            writer.Write('\t');
                            writer.Write('[');
                            writer.Write(logLevel.ToString());
                            writer.Write(']');
                            writer.Write('\t');
                            writer.Write(message);
                            if (exception != null)
                            {
                                writer.Write('\t');
                                writer.Write(exception.ToString());
                            }
                            writer.WriteLine();
                            break;
                        }
                        else
                        {
                            _currentFileNameSuffix++;
                        }
                    }
                }
            }
        }
    }
}
