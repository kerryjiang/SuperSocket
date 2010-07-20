using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;


namespace SuperSocket.Common
{
    public class ELLogger : ILogger
    {
        private LogWriter m_Writer;

        public ELLogger(IEnumerable<string> categories, IEnumerable<string> levels)
        {
            List<ILogFilter> filters = new List<ILogFilter>();

            foreach (var cate in categories)
            {
                foreach(var level in levels)
                {
                    filters.Add(new CategoryFilter(cate + "." + level,
                        new List<string> { cate, level },
                        CategoryFilterMode.DenyAllExceptAllowed));
                }
            }

            List<LogSource> logSources = new List<LogSource>();

            foreach (var cate in categories)
            {
                foreach (var level in levels)
                {
                    logSources.Add(new LogSource(cate + "." + level, new List<TraceListener>
                        {
                            new RollingFlatFileTraceListener(string.Format(@"Logs\{0}\{1}.log", cate, level), "", "", null, 10000, "", RollFileExistsBehavior.Overwrite, RollInterval.Day)
                        }, SourceLevels.All));
                }
            }

            m_Writer = new LogWriter(filters, logSources, null, "Info");
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

        public void LogError(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogError(string server, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = server + " - " + message;
            entry.Categories.Add(server);
            entry.Categories.Add("Error");
            Logger.Write(entry);
        }

        public void LogError(string server, string title, Exception e)
        {
            LogEntry entry = new LogEntry();

            entry.Message = server + " - " + title + " - " + e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = entry.Message + Environment.NewLine + e.InnerException.Message + Environment.NewLine + e.StackTrace;
            }

            entry.Categories.Add(server);
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

        public void LogDebug(string server, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = server + " - " + message;
            entry.Categories.Add(server);
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

        public void LogInfo(string server, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = server + " - " + message;
            entry.Categories.Add(server);
            entry.Categories.Add("Info");
            Logger.Write(entry);
        }

        #endregion
    }
}
