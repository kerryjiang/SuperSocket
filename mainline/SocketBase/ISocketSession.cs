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

    public interface ISocketSession : ISessionBase
    {
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
        /// Sends the message to client.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendResponse(string message);

        /// <summary>
        /// Sends the binary data to client.
        /// </summary>
        /// <param name="data">The binary data should be sent to client.</param>
        void SendResponse(byte[] data);

        /// <summary>
        /// Sends the binary data to client.
        /// </summary>
        /// <param name="data">The binary data should be sent to client.</param>
        void SendResponse(byte[] data, Int32 offset, Int32 length);

        /// <summary>
        /// Receives the data of specific length from socket.
        /// </summary>
        /// <param name="storeSteram">The store steram.</param>
        /// <param name="length">The length.</param>
        void ReceiveData(Stream storeSteram, int length);

        /// <summary>
        /// Receives data from socket until find a binary mark.
        /// </summary>
        /// <param name="storeSteram">The store steram.</param>
        /// <param name="endMark">The end mark.</param>
        void ReceiveData(Stream storeSteram, byte[] endMark);

        /// <summary>
        /// Applies the secure protocol.
        /// </summary>
        void ApplySecureProtocol();

        /// <summary>
        /// Gets the client socket.
        /// </summary>
        Socket Client { get; }

        /// <summary>
        /// Gets the underly stream of the socket connection, only supported in Sync mode.
        /// </summary>
        /// <returns></returns>
        Stream GetUnderlyStream();

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
    }

    public interface ISocketSession<TAppSession> : ISocketSession
        where TAppSession : IAppSession, new()
    {
        /// <summary>
        /// Initializes the specified socket session by AppServer and AppSession.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        void Initialize(IAppServer<TAppSession> appServer, TAppSession appSession);
    }
}
