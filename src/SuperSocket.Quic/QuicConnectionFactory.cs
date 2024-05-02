using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Quic.Connection;
using SuperSocket.Server.Abstractions;

#pragma warning disable CA2252
namespace SuperSocket.Quic
{
    internal class QuicConnectionFactory : IConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly ListenOptions _listenOptions;
        private readonly ConnectionOptions _connectionOptions;

        public QuicConnectionFactory(
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions)
        {
            _listenOptions = listenOptions;
            _connectionOptions = connectionOptions;
            _logger = connectionOptions.Logger;
        }

        public Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var quicConnection = connection as QuicConnection;

            var pipcPipeConnection = new QuicPipeConnection(quicConnection, _connectionOptions);

            pipcPipeConnection.AcceptInboundStream(cancellationToken);

            return Task.FromResult<IConnection>(pipcPipeConnection);
        }
    }
}
#pragma warning restore CA2252