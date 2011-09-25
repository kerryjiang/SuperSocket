using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    public static class LoggerExtension
    {
        private readonly static string m_SessionInfoTemplate = "Session: {0}/{1}";

        public static void LogError(this ILogger logger, ISessionBase session, Exception e)
        {
            logger.LogError(string.Format(m_SessionInfoTemplate, session.IdentityKey, session.RemoteEndPoint), e);   
        }

        public static void LogError(this ILogger logger, ISessionBase session, string title, Exception e)
        {
            logger.LogError(string.Format(m_SessionInfoTemplate, session.IdentityKey, session.RemoteEndPoint) + Environment.NewLine + title, e);
        }

        public static void LogError(this ILogger logger, ISessionBase session, string message)
        {
            logger.LogError(string.Format(m_SessionInfoTemplate, session.IdentityKey, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        public static void LogInfo(this ILogger logger, ISessionBase session, string message)
        {
            string info = string.Format(m_SessionInfoTemplate, session.IdentityKey, session.RemoteEndPoint) + Environment.NewLine + message;
            logger.LogInfo(info);
        }

        public static void LogDebug(this ILogger logger, ISessionBase session, string message)
        {
            logger.LogDebug(string.Format(m_SessionInfoTemplate, session.IdentityKey, session.RemoteEndPoint) + Environment.NewLine + message);
        }
    }
}
