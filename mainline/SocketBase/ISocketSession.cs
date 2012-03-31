using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// CloseReason enum
    /// </summary>
    public enum CloseReason
    {
        /// <summary>
        /// Close for server shutdown
        /// </summary>
        ServerShutdown,

        /// <summary>
        /// The client close the socket
        /// </summary>
        ClientClosing,

        /// <summary>
        /// The server side close the socket
        /// </summary>
        ServerClosing,

        /// <summary>
        /// The socket is closed for a socket error
        /// </summary>
        SocketError,

        /// <summary>
        /// The socket is closed by server for timeout
        /// </summary>
        TimeOut,

        /// <summary>
        /// The socket is closed for unknown reason
        /// </summary>
        Unknown
    }

    /// <summary>
    /// SocketSession closed event argument
    /// </summary>
    public class SocketSessionClosedEventArgs : EventArgs
    {

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>
        /// The session ID.
        /// </value>
        public string SessionID { get; set; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public CloseReason Reason { get; set; }
    }

    /// <summary>
    /// The interface for socket session
    /// </summary>
    public interface ISocketSession : ISessionBase
    {
        /// <summary>
        /// Initializes the specified app session.
        /// </summary>
        /// <param name="appSession">The app session.</param>
        void Initialize(IAppSession appSession);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Closes the socket session for the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        void Close(CloseReason reason);

        /// <summary>
        /// Sends the binary data to client.
        /// </summary>
        /// <param name="data">The binary data should be sent to client.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void SendResponse(byte[] data, int offset, int length);

        /// <summary>
        /// Applies the secure protocol.
        /// </summary>
        void ApplySecureProtocol();

        /// <summary>
        /// Gets the client socket.
        /// </summary>
        Socket Client { get; }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>
        /// The secure protocol.
        /// </value>
        SslProtocols SecureProtocol { get; set; }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        event EventHandler<SocketSessionClosedEventArgs> Closed;

        /// <summary>
        /// Gets the app session assosiated with this socket session.
        /// </summary>
        IAppSession AppSession { get; }
    }
}
