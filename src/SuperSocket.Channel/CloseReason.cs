using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public enum CloseReason
    {
        /// <summary>
        /// The socket is closed for unknown reason
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Close for server shutdown
        /// </summary>
        ServerShutdown = 1,

        /// <summary>
        /// The close behavior is initiated from the remote endpoint
        /// </summary>
        RemoteClosing = 2,

        /// <summary>
        /// The close behavior is initiated from the local endpoint
        /// </summary>
        LocalClosing = 3,

        /// <summary>
        /// Application error
        /// </summary>
        ApplicationError = 4,

        /// <summary>
        /// The socket is closed for a socket error
        /// </summary>
        SocketError = 5,

        /// <summary>
        /// The socket is closed by server for timeout
        /// </summary>
        TimeOut = 6,

        /// <summary>
        /// Protocol error 
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// SuperSocket internal error
        /// </summary>
        InternalError = 8,
    }

    public class CloseEventArgs : EventArgs
    {
        public CloseReason Reason { get; private set; }

        public CloseEventArgs(CloseReason reason)
        {
            Reason = reason;
        }
    }
}