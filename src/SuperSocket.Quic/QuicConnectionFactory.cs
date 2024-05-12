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

        var quicStream = await quicConnection.AcceptInboundStreamAsync(CancellationToken.None);
        
        return new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint, quicConnection.LocalEndPoint, _connectionOptions);
        
        // var quicStream = new QuicPipeStream(quicConnection);
        //
        // var pipeConnection = new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint, quicConnection.LocalEndPoint, _connectionOptions);
        //
        // return Task.FromResult<IConnection>(pipeConnection);
    }
}