using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Logger extension class
    /// </summary>
    public static class LoggerExtension
    {
        private readonly static string m_SessionInfoTemplate = "Session: {0}/{1}";

        /// <summary>
        /// Logs the error with the session's information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="title">The title.</param>
        /// <param name="e">The e.</param>
        /// <param name="session">The session.</param>
        public static void Error(this ILog logger, string title, Exception e, ISessionBase session)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + title, e);
        }

        /// <summary>
        /// Logs the error with the session's information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        public static void Error(this ILog logger, string message, ISessionBase session)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        /// <summary>
        /// Logs the information with the session's information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        public static void Info(this ILog logger, string message, ISessionBase session)
        {
            string info = string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message;
            logger.Info(info);
        }

        /// <summary>
        /// Logs the debug message with the session's information
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        public static void Debug(this ILog logger, string message, ISessionBase session)
        {
            if (!logger.IsDebugEnabled)
                return;

            logger.Debug(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        private const string m_PerfLogName = "Perf";

        private static ILog m_PerfLog;

        /// <summary>
        /// Logs the performance message
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="message">The message.</param>
        public static void LogPerf(this IAppServer appServer, string message)
        {
            if (m_PerfLog == null)
            {
                lock (m_PerfLogName)
                {
                    if (m_PerfLog == null)
                    {
                        m_PerfLog = appServer.LogFactory.GetLog(m_PerfLogName);
                    }
                }
            }

            if (m_PerfLog != null && m_PerfLog.IsInfoEnabled)
                m_PerfLog.Info(message);
        }
    }
}
