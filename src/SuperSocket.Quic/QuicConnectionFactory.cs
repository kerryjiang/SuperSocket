using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Quic.Connection;
using SuperSocket.Server.Abstractions;

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

        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var quicConnection = (QuicConnection)connection;

            var quicStream = new QuicPipeStream(quicConnection);

            var pipeConnection = new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint, quicConnection.LocalEndPoint, _connectionOptions);
            
            return Task.FromResult<IConnection>(pipeConnection);
        }
    }
}
