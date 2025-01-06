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
    public class TcpConnectionFactory : TcpConnectionFactoryBase
    {
        private readonly ObjectPool<SocketSender> _socketSenderPool;

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