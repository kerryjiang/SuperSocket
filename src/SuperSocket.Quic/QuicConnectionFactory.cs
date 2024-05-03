using System.Collections.Generic;
using System.IO;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
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

        public QuicConnectionFactory(ListenOptions listenOptions, ConnectionOptions connectionOptions)
        {
            _listenOptions = listenOptions;
            _connectionOptions = connectionOptions;
        }

        public async Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var quicConnection = connection as QuicConnection;

            var quicStream = await quicConnection.AcceptInboundStreamAsync(CancellationToken.None);

            // var quicStream = new QuicPipeStream(quicConnection, new QuicStreamOptions
            // {
            //     ServerStream = true,
            // });

            return new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint, quicConnection.LocalEndPoint,
                _connectionOptions);
        }
    }
}