using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common
{
    public class LogUtil
    {
        private static ILogger m_logger = new EventLogger();

        public static void Setup(ILogger logger)
        {
            m_logger = logger;
        }

        public static void LogError(Exception e)
        {
            m_logger.LogError(e);
        }

        public static void LogError(ILogApp app, Exception e)
        {
            m_logger.LogError(app, e);
        }

        public static void LogError(string title, Exception e)
        {
            m_logger.LogError(title, e);
        }

        public static void LogError(ILogApp app, string title, Exception e)
        {
            m_logger.LogError(app, title, e);
        }

        public static void LogError(string message)
        {
            m_logger.LogError(message);
        }

        public static void LogError(ILogApp app, string message)
        {
            m_logger.LogError(app, message);
        }

        public static void LogDebug(string message)
        {
            m_logger.LogDebug(message);
        }

        public static void LogDebug(ILogApp app, string message)
        {
            m_logger.LogDebug(app, message);
        }

        public static void LogInfo(string message)
        {
            m_logger.LogInfo(message);
        }

        public static void LogInfo(ILogApp app, string message)
        {
            m_logger.LogInfo(app, message);
        }
    }
}
