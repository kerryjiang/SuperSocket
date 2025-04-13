using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.ObjectPool;
using SuperSocket.Connection;

namespace SuperSocket.Client
{
    /// <summary>
    /// Represents the state of a connection, including its result, socket, and stream.
    /// </summary>
    public class ConnectState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectState"/> class.
        /// </summary>
        public ConnectState()
        {
        }

        private ConnectState(bool cancelled)
        {
            Cancelled = cancelled;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection was successful.
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Gets a value indicating whether the connection was cancelled.
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Gets or sets the exception that occurred during the connection attempt, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the socket associated with the connection.
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// Gets or sets the stream associated with the connection.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Represents a connection state that indicates the connection was cancelled.
        /// </summary>
        public static readonly ConnectState CancelledState = new ConnectState(false);

        private static Lazy<ObjectPool<SocketSender>> _socketSenderPool = new Lazy<ObjectPool<SocketSender>>(() =>
        {
            var policy = new DefaultPooledObjectPolicy<SocketSender>();
            var pool = new DefaultObjectPool<SocketSender>(policy, EasyClient.SocketSenderPoolSize ?? EasyClient.DefaultSocketSenderPoolSize);
            return pool;
        });

        /// <summary>
        /// Creates a connection object based on the current state.
        /// </summary>
        /// <param name="connectionOptions">The connection options to use.</param>
        /// <returns>An <see cref="IConnection"/> object representing the connection.</returns>
        public IConnection CreateConnection(ConnectionOptions connectionOptions)
        {
            var stream = this.Stream;
            var socket = this.Socket;

            if (stream != null)
            {
                return new StreamPipeConnection(stream , socket.RemoteEndPoint, socket.LocalEndPoint, connectionOptions);
            }
            else
            {
                return new TcpPipeConnection(socket, connectionOptions, _socketSenderPool.Value);
            }
        }
    }
}