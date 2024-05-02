using System;
using System.IO;
using System.Net;
using SuperSocket.Connection;

namespace SuperSocket.Quic.Connection;

public class QuicPipeConnection : StreamPipeConnection
{
    public QuicPipeConnection(Stream stream, EndPoint remoteEndPoint, ConnectionOptions options) 
        : base(stream, remoteEndPoint, options)
    {
    }

    public QuicPipeConnection(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, ConnectionOptions options) 
        : base(stream, remoteEndPoint, localEndPoint, options)
    {
    }

    protected override bool IsIgnorableException(Exception e)
    {
        return base.IsIgnorableException(e);
    }
}