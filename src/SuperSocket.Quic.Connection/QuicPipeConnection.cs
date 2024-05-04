using System;
using System.IO;
using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

#pragma warning disable CA2252

namespace SuperSocket.Quic.Connection;

public class QuicPipeConnection : StreamPipeConnection
{
    private readonly Stream _stream;

    public QuicPipeConnection(Stream stream, EndPoint remoteEndPoint, ConnectionOptions options)
        : this(stream, remoteEndPoint, null, options)
    {
    }

    public QuicPipeConnection(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, ConnectionOptions options)
        : base(stream, remoteEndPoint, localEndPoint, options)
    {
        if (stream is not QuicStream or QuicPipeStream)
            throw new NotSupportedException("QuicPipeConnection only supports QuicStream or QuicPipeStream");

        _stream = stream;
    }

    protected override async Task StartInputPipeTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe,
        CancellationToken cancellationToken)
    {
        if (_stream is QuicPipeStream quicPipeStream)
            await quicPipeStream.OpenStreamAsync(cancellationToken);

        await base.StartInputPipeTask(packagePipe, cancellationToken);
    }

    protected override bool IsIgnorableException(Exception e)
    {
        if (base.IsIgnorableException(e))
            return true;

        switch (e)
        {
            case QuicException:
            case SocketException se when se.IsIgnorableSocketException():
                return true;
            default:
                return false;
        }
    }
}
