using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    abstract partial class SocketSession
    {
        private const string m_GeneralErrorMessage = "Unexpected error";
        private const string m_GeneralSocketErrorMessage = "Unexpected socket error: {0}";

        /// <summary>
        /// Logs the error, skip the ignored exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void LogError(Exception exception)
        {
            int socketErrorCode;

            //This exception is ignored, needn't log it
            if (IsIgnorableException(exception, out socketErrorCode))
                return;

            var message = socketErrorCode > 0 ? string.Format(m_GeneralSocketErrorMessage, socketErrorCode) : m_GeneralErrorMessage;

            AppSession.Logger.Error(this, message, exception);
        }

        /// <summary>
        /// Logs the error, skip the ignored exception
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        protected void LogError(string message, Exception exception)
        {
            int socketErrorCode;

            //This exception is ignored, needn't log it
            if (IsIgnorableException(exception, out socketErrorCode))
                return;

            AppSession.Logger.Error(this, message, exception);
        }

        /// <summary>
        /// Logs the socket error, skip the ignored error
        /// </summary>
        /// <param name="socketErrorCode">The socket error code.</param>
        protected void LogError(int socketErrorCode)
        {
            if (!Config.LogAllSocketException)
            {
                //This error is ignored, needn't log it
                if (IsIgnorableSocketError(socketErrorCode))
                    return;
            }

            AppSession.Logger.Error(this, string.Format(m_GeneralSocketErrorMessage, socketErrorCode), new SocketException(socketErrorCode));
        }
    }
}
