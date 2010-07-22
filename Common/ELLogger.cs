using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SuperSocket.Common
{
    public class ELLogger : ILogger
    {
        public ELLogger()
        {

        }

        #region ILogger Members

        public void LogError(Exception e)
        {
            LogEntry entry = new LogEntry();
            entry.Message = e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = e.Message + Environment.NewLine + e.InnerException.Message
                    + Environment.NewLine + e.InnerException.StackTrace;
            }

            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogError(ILogApp app, Exception e)
        {
            LogEntry entry = new LogEntry();

            entry.Message = app.Name + " - " + e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = e.Message + Environment.NewLine + e.InnerException.Message
                    + Environment.NewLine + e.InnerException.StackTrace;
            }

            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogError(string title, Exception e)
        {
            LogEntry entry = new LogEntry();

            entry.Message = title + " - " + e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = entry.Message + Environment.NewLine + e.InnerException.Message + Environment.NewLine + e.StackTrace;
            }

            entry.Categories.Add("Error");

            Logger.Write(entry);
        }

        public void LogError(ILogApp app, string title, Exception e)
        {
            LogEntry entry = new LogEntry();

            entry.Message = app.Name + " - " + title + " - " + e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = entry.Message + Environment.NewLine + e.InnerException.Message + Environment.NewLine + e.StackTrace;
            }

            entry.Categories.Add("Error");

            Logger.Write(entry);
        }

        public void LogError(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogError(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = app.Name + " - " + message;
            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogDebug(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Debug");
            Logger.Write(entry);
        }

        public void LogDebug(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = app.Name + " - " + message;
            entry.Categories.Add("Debug");
            Logger.Write(entry);
        }

        public void LogInfo(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Info");
            Logger.Write(entry);
        }

        public void LogInfo(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = app.Name + " - " + message;
            entry.Categories.Add("Info");
            Logger.Write(entry);
        }

        public void LogPerf(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Perf");
            Logger.Write(entry);
        }

        public void LogPerf(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = app.Name + " - " + message;
            entry.Categories.Add("Perf");
            Logger.Write(entry);
        }

        #endregion
    }
}
