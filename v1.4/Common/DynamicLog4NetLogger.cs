using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace SuperSocket.Common
{
    public class DynamicLog4NetLogger : Log4NetLogger
    {
        private static IAppender[] m_RawAppenders;

        static DynamicLog4NetLogger()
        {
            m_RawAppenders = log4net.LogManager.GetRepository().GetAppenders();
        }

        public DynamicLog4NetLogger(string name)
            : this(name, null)
        {

        }

        public DynamicLog4NetLogger(string name, ILogger nestedLogger)
            : base(name, nestedLogger)
        {
            AdaptAppenderFilePath();
        }

        private void AdaptAppenderFilePath()
        {
            var currentLogger = Logger.Logger as log4net.Repository.Hierarchy.Logger;
            currentLogger.Additivity = false;//disable root appender for this logger

            foreach (var ap in m_RawAppenders)
            {
                //Always use shared perf appender
                if (ap.Name.StartsWith("perf", StringComparison.OrdinalIgnoreCase))
                {
                    currentLogger.AddAppender(ap);
                    continue;
                }

                var rollingFileAppender = CloneAppender(ap as RollingFileAppender);
                if (rollingFileAppender == null)
                    continue;

                rollingFileAppender.File = ProcessLogFilePath(rollingFileAppender.File);
                currentLogger.AddAppender(rollingFileAppender);
                rollingFileAppender.ActivateOptions();
            }
        }

        private string ProcessLogFilePath(string rawFilePath)
        {
            var fileNodes = rawFilePath.Split('/', '\\').ToList();
            fileNodes.Insert(fileNodes.Count - 1, Name);
            return string.Join(Path.DirectorySeparatorChar.ToString(), fileNodes.ToArray());
        }

        private RollingFileAppender CloneAppender(RollingFileAppender appender)
        {
            var newAppender = new RollingFileAppender();
            newAppender.Name = appender.Name;
            newAppender.RollingStyle = appender.RollingStyle;
            newAppender.StaticLogFileName = appender.StaticLogFileName;
            newAppender.Threshold = appender.Threshold;
            newAppender.MaxSizeRollBackups = appender.MaxSizeRollBackups;
            newAppender.MaximumFileSize = appender.MaximumFileSize;
            newAppender.MaxFileSize = appender.MaxFileSize;
            //newAppender.LockingModel = appender.LockingModel;
            newAppender.Layout = appender.Layout;
            newAppender.ImmediateFlush = appender.ImmediateFlush;
            newAppender.File = appender.File;
            newAppender.ErrorHandler = appender.ErrorHandler;
            newAppender.Encoding = appender.Encoding;
            newAppender.DatePattern = appender.DatePattern;
            newAppender.CountDirection = appender.CountDirection;
            newAppender.AppendToFile = appender.AppendToFile;
            newAppender.AddFilter(appender.FilterHead);
            return newAppender;
        }
    }
}
