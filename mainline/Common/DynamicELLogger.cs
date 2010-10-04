using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace SuperSocket.Common
{
    public class DynamicELLogger : ILogger
    {
        private LogWriter m_Writer;

        public DynamicELLogger(string logRoot, IEnumerable<string> applications)
        {
            string[] categories = new string[] { "Info", "Error", "Debug", "Perf" };

            LoggingSettings loggingSetting = LoggingSettings.GetLoggingSettings(ConfigurationSourceFactory.Create());

            Dictionary<string, TextFormatter> formatters = new Dictionary<string, TextFormatter>(categories.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (string cate in categories)
            {
                var formatData = loggingSetting.Formatters.Where(f => f.Name.Equals(cate, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() as TextFormatterData;

                if (formatData == null)
                    throw new Exception(string.Format("Missing logging formatter \"{0}\"", cate));

                TextFormatter formatter = new TextFormatter(formatData.Template);
                formatters[cate] = formatter;
            }

            string baseLogPath = Path.Combine(logRoot, "{0}.log");
            string logPath = Path.Combine(logRoot, "{0}\\{1}.log");

            List<LogSource> logSources = new List<LogSource>();

            foreach (var cate in categories)
            {
                logSources.Add(new LogSource(cate, new List<TraceListener>
                    {
                        new RollingFlatFileTraceListener(string.Format(baseLogPath, cate), "", "", formatters[cate], 0, "yyyyMMdd", RollFileExistsBehavior.Overwrite, RollInterval.Day)
                    }, SourceLevels.All));
            }

            foreach (var app in applications)
            {
                foreach (var cate in categories)
                {
                    logSources.Add(new LogSource(app + "." + cate, new List<TraceListener>
                        {
                            new RollingFlatFileTraceListener(string.Format(logPath, app, cate), "", "", formatters[cate], 0, "yyyyMMdd", RollFileExistsBehavior.Overwrite, RollInterval.Day)
                        }, SourceLevels.All));
                }
            }

            var nonExistantLog = new LogSource("Empty");

            m_Writer = new LogWriter(new ILogFilter[0], logSources, nonExistantLog, categories[0]);
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
            m_Writer.Write(entry);
        }

        public void LogError(ILogApp app, Exception e)
        {
            LogEntry entry = new LogEntry();
            entry.Message = e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = e.Message + Environment.NewLine + e.InnerException.Message
                    + Environment.NewLine + e.InnerException.StackTrace;
            }

            entry.Categories.Add(app.Name + ".Error");
            m_Writer.Write(entry);
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

            m_Writer.Write(entry);
        }

        public void LogError(ILogApp app, string title, Exception e)
        {
            LogEntry entry = new LogEntry();

            entry.Message = title + " - " + e.Message + Environment.NewLine + e.StackTrace;

            if (e.InnerException != null)
            {
                entry.Message = entry.Message + Environment.NewLine + e.InnerException.Message + Environment.NewLine + e.StackTrace;
            }

            entry.Categories.Add(app.Name + ".Error");

            m_Writer.Write(entry);
        }

        public void LogError(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Error");
            m_Writer.Write(entry);
        }

        public void LogError(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add(app.Name + ".Error");
            m_Writer.Write(entry);
        }

        public void LogDebug(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Debug");
            m_Writer.Write(entry);
        }

        public void LogDebug(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add(app.Name + ".Debug");
            m_Writer.Write(entry);
        }

        public void LogInfo(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Info");
            m_Writer.Write(entry);
        }

        public void LogInfo(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add(app.Name + ".Info");
            m_Writer.Write(entry);
        }

        public void LogPerf(string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add("Perf");
            m_Writer.Write(entry);
        }

        public void LogPerf(ILogApp app, string message)
        {
            LogEntry entry = new LogEntry();
            entry.Message = message;
            entry.Categories.Add(app.Name + ".Perf");
            m_Writer.Write(entry);
        }

        #endregion
    }
}
