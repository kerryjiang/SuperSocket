using System.Collections.Generic;
using System.IO;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Quic.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

#pragma warning disable CA2252
namespace SuperSocket.Quic
{
    internal class QuicConnectionFactory : IConnectionFactory
    {
        private readonly ListenOptions _listenOptions;
        private readonly ConnectionOptions _connectionOptions;
        private readonly IEnumerable<IConnectionStreamInitializer> _connectionStreamInitializers;

        public QuicConnectionFactory(
            IConnectionStreamInitializersFactory connectionStreamInitializersFactory,
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions)
        {
            _listenOptions = listenOptions;
            _connectionOptions = connectionOptions;
            _connectionStreamInitializers = connectionStreamInitializersFactory.Create(_listenOptions);
        }

        public async Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            Stream stream = null;
            var quicConnection = connection as QuicConnection;

            foreach (var initializer in _connectionStreamInitializers)
            {
                stream = await initializer.InitializeAsync(quicConnection, cancellationToken);
            }

            var quicStream = (QuicPipeStream)stream;

            return new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint,
                quicConnection.LocalEndPoint, _connectionOptions);
        }
    }
}
