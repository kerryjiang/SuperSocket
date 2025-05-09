using System;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Specifies the reasons for closing a connection.
    /// </summary>
    public enum CloseReason
    {
        /// <summary>
        /// The socket is closed for an unknown reason.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The connection is closed due to server shutdown.
        /// </summary>
        ServerShutdown = 1,

        /// <summary>
        /// The connection is closed by the remote endpoint.
        /// </summary>
        RemoteClosing = 2,

        /// <summary>
        /// The connection is closed by the local endpoint.
        /// </summary>
        LocalClosing = 3,

        /// <summary>
        /// The connection is closed due to an application error.
        /// </summary>
        ApplicationError = 4,

        /// <summary>
        /// The connection is closed due to a socket error.
        /// </summary>
        SocketError = 5,

        /// <summary>
        /// The connection is closed by the server due to a timeout.
        /// </summary>
        TimeOut = 6,

        /// <summary>
        /// The connection is closed due to a protocol error.
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// The connection is closed due to an internal error in SuperSocket.
        /// </summary>
        InternalError = 8,

        /// <summary>
        /// The connection is closed because it was rejected.
        /// </summary>
        Rejected = 9
    }

    /// <summary>
    /// Provides data for connection close events.
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason for the connection closure.
        /// </summary>
        public CloseReason Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseEventArgs"/> class with the specified close reason.
        /// </summary>
        /// <param name="reason">The reason for the connection closure.</param>
        public CloseEventArgs(CloseReason reason)
        {
            Reason = reason;
        }
    }
}