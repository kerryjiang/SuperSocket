using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for endpoint who can send/receive system message with each other
    /// </summary>
    public interface ISystemEndPoint
    {
        /// <summary>
        /// Transfers the system message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageData">The message data.</param>
        void TransferSystemMessage(string messageType, object messageData);
    }
}
