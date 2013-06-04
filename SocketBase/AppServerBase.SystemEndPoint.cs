using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TRequestInfo> : ISystemEndPoint
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Transfers the system message
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageData">The message data.</param>
        void ISystemEndPoint.TransferSystemMessage(string messageType, object messageData)
        {
            OnSystemMessageReceived(messageType, messageData);
        }

        /// <summary>
        /// Called when [system message received].
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageData">The message data.</param>
        protected virtual void OnSystemMessageReceived(string messageType, object messageData)
        {

        }
    }
}
