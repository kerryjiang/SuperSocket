using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Factory for creating TCP connections with optional stream initializers and socket options.
    /// </summary>
    public class TcpConnectionFactory : TcpConnectionFactoryBase
    {
        private readonly ObjectPool<SocketSender> _socketSenderPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnectionFactory"/> class.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <param name="connectionOptions">The options for the connection.</param>
        /// <param name="socketOptionsSetter">An action to configure socket options.</param>
        /// <param name="connectionStreamInitializersFactory">The factory for creating connection stream initializers.</param>
        public TcpConnectionFactory(
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions,
            Action<Socket> socketOptionsSetter,
            IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
            : base(listenOptions, connectionOptions, socketOptionsSetter, connectionStreamInitializersFactory)
        {
            if (!(connectionOptions.Values?.TryGetValue("socketSenderPoolSize", out var socketSenderPoolSize) == true && int.TryParse(socketSenderPoolSize, out var socketSenderPoolSizeValue)))
            {
                socketSenderPoolSizeValue = 1000;
            }

            _socketSenderPool = new DefaultObjectPool<SocketSender>(new DefaultPooledObjectPolicy<SocketSender>(), socketSenderPoolSizeValue);
        }

        /// <summary>
        /// Creates a new connection asynchronously.
        /// </summary>
        /// <param name="connection">The connection object, typically a socket.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created connection.</returns>
        public override async Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var socket = connection as Socket;

            ApplySocketOptions(socket);

            if (ConnectionStreamInitializers is IEnumerable<IConnectionStreamInitializer> connectionStreamInitializers
                && connectionStreamInitializers.Any())
            {
                var stream = default(Stream);

                foreach (var initializer in connectionStreamInitializers)
                {
                    stream = await initializer.InitializeAsync(socket, stream, cancellationToken);
                }

                return new StreamPipeConnection(stream, socket.RemoteEndPoint, socket.LocalEndPoint, ConnectionOptions);
            }

            return new TcpPipeConnection(socket, ConnectionOptions, _socketSenderPool);
        }
    }
}