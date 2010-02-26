using System;
using System.Diagnostics;

namespace GiantSoft.Common
{
    public class EventLogger : ILogger
    {
        #region ILogger Members

        public void LogError(Exception e)
        {
			EventLog.WriteEntry("GiantSoft.Common.EventLogger.Error", e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(string title, Exception e)
        {
            EventLog.WriteEntry("GiantSoft.Common.EventLogger.Error", title + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace, EventLogEntryType.Error);
        }

        public void LogError(string message)
        {
            EventLog.WriteEntry("GiantSoft.Common.EventLogger.Error", message, EventLogEntryType.Error);
        }

        public void LogDebug(string message)
        {
            EventLog.WriteEntry("GiantSoft.Common.EventLogger.Debug", message, EventLogEntryType.Information);
        }

        public void LogInfo(string message)
        {
			EventLog.WriteEntry("GiantSoft.Common.EventLogger.Info", message, EventLogEntryType.Information);
        }

        #endregion
    }
}
