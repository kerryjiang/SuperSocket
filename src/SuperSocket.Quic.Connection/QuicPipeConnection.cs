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
    public QuicPipeConnection(QuicStream stream, EndPoint remoteEndPoint, ConnectionOptions options)
        : this(stream, remoteEndPoint, null, options)
    {
    }

    public QuicPipeConnection(QuicStream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, ConnectionOptions options)
        : base(stream, remoteEndPoint, localEndPoint, options)
    {
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
