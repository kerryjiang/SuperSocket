using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net.Config;

namespace SuperSocket.Common
{
    public class LogUtil
    {
        private static ILogger m_logger;

        public static void Setup()
        {
            if(Path.DirectorySeparatorChar == '\\') // Windows
                Setup(Path.Combine("Config", "log4net.config"));
            else //Unix
                Setup(Path.Combine("Config", "log4net.unix.config"));
        }

        public static void Setup(string log4netConfig)
        {
            if (!Path.IsPathRooted(log4netConfig))
                log4netConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, log4netConfig);

            XmlConfigurator.Configure(new FileInfo(log4netConfig));
            m_logger = new Log4NetLogger();
        }

        public static void Setup(ILogger logger)
        {
            m_logger = logger; 
        }

        public static ILogger GetRootLogger()
        {
            return m_logger;
        }

        public static void LogError(Exception e)
        {
            if (m_logger != null)
                m_logger.LogError(e);
        }

        public static void LogError(string title, Exception e)
        {
            if (m_logger != null)
                m_logger.LogError(title, e);
        }

        public static void LogError(string message)
        {
            if (m_logger != null)
                m_logger.LogError(message);
        }

        public static void LogDebug(string message)
        {
            if (m_logger != null)
                m_logger.LogDebug(message);
        }

        public static void LogInfo(string message)
        {
            if (m_logger != null)
                m_logger.LogInfo(message);
        }

        public static void LogPerf(string message)
        {
            if (m_logger != null)
                m_logger.LogPerf(message);
        }
    }
}
