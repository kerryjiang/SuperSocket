using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Quic.Connection;

#pragma warning disable CA2252
namespace SuperSocket.Quic;

internal class QuicConnectionFactory : IConnectionFactory
{
    private readonly ConnectionOptions _connectionOptions;

    public QuicConnectionFactory(ConnectionOptions connectionOptions)
    {
        _connectionOptions = connectionOptions;
    }

    public async Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
    {
        var quicConnection = (QuicConnection)connection;

        var stream = await quicConnection.AcceptInboundStreamAsync(cancellationToken);

        return new QuicPipeConnection(stream, quicConnection.RemoteEndPoint,
            quicConnection.LocalEndPoint, _connectionOptions);
    }
}