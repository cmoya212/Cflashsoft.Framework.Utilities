using System;
using System.Collections.Generic;
using System.Text;

namespace Cflashsoft.Framework.Logging
{
    /// <summary>
    /// Represents options to configure CfxFileLogger.
    /// </summary>
    public class CfFxFileLoggerOptions
    {
        /// <summary>
        /// Prefix text to prepend to the log files that indicate the application.
        /// </summary>
        public string LogFilePrefix { get; set; }
        /// <summary>
        /// Destination directory on disk for the log files.
        /// </summary>
        public string LogFilePath { get; set; }
        /// <summary>
        /// Indicates whether log files are rolled over on a daily basis with a new date in the name.
        /// </summary>
        public bool? DailyRollover { get; set; }
        /// <summary>
        /// Maximum size of a single log file. If the size is exceeded a new log file will be created with sequential number.
        /// </summary>
        public int? MaxLogFileSize { get; set; }
        /// <summary>
        /// Custom application function to call when a record is written to the log file.
        /// </summary>
        public LogAction WriteFinishedAction { get; set; }
        /// <summary>
        /// Custom application function to call when a record could not be written to the log file.
        /// </summary>
        public LogAction WriteFailureAction { get; set; }
    }
}
