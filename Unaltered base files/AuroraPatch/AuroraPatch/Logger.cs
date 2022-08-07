using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace AuroraPatch
{
    /// <summary>
    /// Enum representing various log levels.
    /// </summary>
    internal enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4,
    }

    /// <summary>
    /// Logging wrapper so we can easily switch logging libraries/configuration in the future if necessary.
    /// </summary>
    internal class Logger
    {
        private string Name;
        private LogLevel Level;
        public Logger(LogLevel level = LogLevel.Info, string name = "AuroraPatch", string logFile = "AuroraPatch.log")
        {
            Name = name;
            Level = level;
            Stream logStream = File.Create(logFile);
            TextWriterTraceListener traceListener = new TextWriterTraceListener(logStream, "AuroraPatch");
            Trace.Listeners.Add(traceListener);
            Trace.AutoFlush = true;
            LogInfo("AuroraPatch logger initialized");
        }

        private void Log(string message, LogLevel level, bool popup = false)
        {
            Trace.WriteLine(
                string.Format("{0} - {1} - {2}: {3}",
                    DateTime.Now,
                    Name,
                    Enum.GetName(typeof(LogLevel), level),
                    message
                )
            );
            if (popup)
            {
                MessageBox.Show(
                    string.Format("{0}: {1}",
                        Enum.GetName(typeof(LogLevel), level),
                        message
                    )
                );
            }
        }

        public void LogDebug(string message)
        {
            if (Level <= LogLevel.Debug) Log(message, LogLevel.Debug, false);
        }

        public void LogInfo(string message, bool popup = false)
        {
            if (Level <= LogLevel.Info) Log(message, LogLevel.Info, popup);
        }

        public void LogWarning(string message, bool popup = false)
        {
            if (Level <= LogLevel.Warning) Log(message, LogLevel.Warning, popup);
        }

        public void LogError(string message, bool popup = false)
        {
            if (Level <= LogLevel.Error) Log(message, LogLevel.Error, popup);
        }

        public void LogCritical(string message, bool popup = false)
        {
            if (Level <= LogLevel.Critical) Log(message, LogLevel.Critical, popup);
        }
    }
}
