using System;
using System.Diagnostics;

namespace SuperSocket.Common
{
    public class EventLogger : ILogger
    {
        #region ILogger Members

        public void LogError(Exception e)
        {
            EventLog.WriteEntry("SuperSocket.", e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(ILogApp app, Exception e)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(string title, Exception e)
        {
            EventLog.WriteEntry("SuperSocket", title + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(ILogApp app, string title, Exception e)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, title + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(string message)
        {
            EventLog.WriteEntry("SuperSocket.", message, EventLogEntryType.Error);
        }

        public void LogError(ILogApp app, string message)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, message, EventLogEntryType.Error);
        }

        public void LogDebug(string message)
        {
            EventLog.WriteEntry("SuperSocket.", message, EventLogEntryType.Information);
        }

        public void LogDebug(ILogApp app, string message)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, message, EventLogEntryType.Information);
        }

        public void LogInfo(string message)
        {
            EventLog.WriteEntry("SuperSocket.", message, EventLogEntryType.Information);
        }

        public void LogInfo(ILogApp app, string message)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, message, EventLogEntryType.Information);
        }

        public void LogPerf(string message)
        {
            EventLog.WriteEntry("SuperSocket.", message, EventLogEntryType.Information);
        }

        public void LogPerf(ILogApp app, string message)
        {
            EventLog.WriteEntry("SuperSocket." + app.Name, message, EventLogEntryType.Information);
        }

        #endregion
    }
}
