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
    public enum CloseReason : int
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
        /// The client close the socket
        /// </summary>
        ClientClosing = 2,

        /// <summary>
        /// The server side close the socket
        /// </summary>
        ServerClosing = 3,

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
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segments">The segments.</param>
        bool TrySend(IList<ArraySegment<byte>> segments);

        /// <summary>
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        bool TrySend(ArraySegment<byte> segment);

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
        Action<ISocketSession, CloseReason> Closed { get; set; }

        /// <summary>
        /// Gets the app session assosiated with this socket session.
        /// </summary>
        IAppSession AppSession { get; }


        /// <summary>
        /// Gets the original receive buffer offset.
        /// </summary>
        /// <value>
        /// The original receive buffer offset.
        /// </value>
        int OrigReceiveOffset { get; }
    }
}
