using System.IO;
using System.Net.Quic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Quic.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Quic;

internal class QuicPipeStreamInitializer : IConnectionStreamInitializer
{
    public void Setup(ListenOptions listenOptions)
    {
    }

    public Task<Stream> InitializeAsync(object connection, CancellationToken cancellationToken)
    {
        var quicConnection = (QuicConnection)connection;
        
        return Task.FromResult<Stream>(new QuicPipeStream(quicConnection, true));
    }
}