using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Cflashsoft.Framework.Logging
{
    public class CfFxFileLogger : ILogger
    {
        private readonly object _logFileSyncLock = new object();
        private readonly string _logFilePath = null;
        private readonly bool _dailyRollover = true;
        private readonly int _maxLogFileSize = 536870912; //512mb

        private int _currentFileNameSuffix = 0;
        private string _currentDateString = null;

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

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (_logFileSyncLock)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
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
                            writer.Write(formatter(state, exception));
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
