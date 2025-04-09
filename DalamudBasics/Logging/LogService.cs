using Dalamud.Plugin.Services;
using DalamudBasics.Logging.Loggers;
using System;
using System.Collections.Concurrent;

namespace DalamudBasics.Logging
{
    internal class LogService : ILogService
    {
        private bool initialized = false;
        private ConcurrentQueue<LogEntryParam> queuedLogEntries = new();

        private readonly IFileLogger fileLogger;
        private readonly IPluginLog pluginLog;

        public void AttachToGameLogicLoop(IFramework framework)
        {
            framework.Update += Tick;
            initialized = true;
        }

        public LogService(IFileLogger fileLogger, IPluginLog pluginLog)
        {
            this.fileLogger = fileLogger;
            this.pluginLog = pluginLog;
        }

        private void QueueEntry(string message, LogLevel logLevel, Exception? exception = null)
        {
            if (!initialized)
            {
                pluginLog.Error($"You forgot to call {nameof(LogService)}.{nameof(AttachToGameLogicLoop)}!");
                return;
            }

            queuedLogEntries.Enqueue(new LogEntryParam
            {
                message = message,
                logLevel = logLevel,
                ex = exception
            });
        }

        public void Debug(string message)
        {
            QueueEntry(message, LogLevel.Debug);
        }

        public void Info(string message)
        {
            QueueEntry(message, LogLevel.Information);
        }

        public void Warning(string message)
        {
            QueueEntry(message, LogLevel.Warning);
        }

        public void Error(string message)
        {
            QueueEntry(message, LogLevel.Error);
        }

        public void Error(Exception ex, string message)
        {
            QueueEntry(message, LogLevel.Error, ex);
        }

        public void Fatal(Exception ex, string message)
        {
            QueueEntry(message, LogLevel.Fatal, ex);
        }

        private void Tick(IFramework framework)
        {
            try
            {
                while (queuedLogEntries.TryDequeue(out LogEntryParam? entry))
                {
                    LogAnEntry(entry);
                }
            }
            catch (Exception ex)
            {
                QueueEntry("Error inside logging queue loop", LogLevel.Error, ex);
            }
        }

        private string AddLogTimeStampAndLogLevel(string message, string logLevelString)
        {
            string messageWithLogLevel = $"[{logLevelString}] {message}";
            return AddTimeStamp(messageWithLogLevel);
        }

        private string AddTimeStamp(string message)
        {
            return $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}] {message}";
        }

        private class LogEntryParam
        {
            public string message;
            public LogLevel logLevel;
            public Exception? ex;
        }

        private enum LogLevel
        {
            Debug, Information, Warning, Error, Fatal
        }

        private void LogAnEntry(LogEntryParam entry)
        {
            LogThroughDalamud(entry);
            LogToFile(entry);
        }

        private void LogToFile(LogEntryParam entry)
        {
            string message = entry.message;
            if (entry.ex != null)
            {
                message = $"{message} Exception: {entry.ex}";
            }
            string logLevelIdentifier = GetLogLevelIdentifier(entry.logLevel);

            fileLogger.Log(AddLogTimeStampAndLogLevel(message, logLevelIdentifier));
        }

        private void LogThroughDalamud(LogEntryParam entry)
        {
#if !DEBUG
            if (entry.logLevel == LogLevel.Debug || entry.logLevel == LogLevel.Information)
            {
                // Don't log into the console low levels, to avoid spamming it, unless i'm debugging.
                return;
            }
#endif
            string message = entry.message;
            switch (entry.logLevel)
            {
                case LogLevel.Debug:
                    pluginLog.Debug(message); break;
                case LogLevel.Information:
                    pluginLog.Info(message); break;
                case LogLevel.Warning:
                    pluginLog.Warning(message); break;
                case LogLevel.Error:
                    if (entry.ex != null)
                    {
                        pluginLog.Error(entry.ex, entry.message);
                        return;
                    }
                    pluginLog.Error(message);
                    return;

                case LogLevel.Fatal:
                    pluginLog.Fatal(entry.ex, message); return;
                default: return;
            }
        }

        private string GetLogLevelIdentifier(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Fatal => "FTL",
                _ => "UNK"
            };
        }
    }
}
