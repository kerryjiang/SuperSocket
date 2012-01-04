using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Common.Logging;

namespace SuperSocket.SocketBase
{
    public static class LoggerExtension
    {
        private readonly static string m_SessionInfoTemplate = "Session: {0}/{1}";

        public static void Error(this ILog logger, ISessionBase session, Exception e)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint), e);
        }

        public static void Error(this ILog logger, ISessionBase session, string title, Exception e)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + title, e);
        }

        public static void Error(this ILog logger, ISessionBase session, string message)
        {
            logger.Error(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        public static void Info(this ILog logger, ISessionBase session, string message)
        {
            string info = string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message;
            logger.Info(info);
        }

        public static void Debug(this ILog logger, ISessionBase session, string message)
        {
            if (!logger.IsDebugEnabled)
                return;

            logger.Debug(string.Format(m_SessionInfoTemplate, session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        private const string m_PerfLogName = "Perf";

        public static void Perf(this IAppServer appServer, string message)
        {
            var perfLog = LogFactoryProvider.LogFactory.GetLog(m_PerfLogName);

            if (perfLog != null && perfLog.IsInfoEnabled)
                perfLog.Info(message);
        }
    }
}
