using System;
using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

#pragma warning disable CA2252

namespace SuperSocket.Quic.Connection
{
    public class QuicPipeConnection : StreamPipeConnection
    {
        private readonly QuicPipeStream _stream;

        public QuicPipeConnection(QuicPipeStream stream, EndPoint remoteEndPoint, ConnectionOptions options)
            : this(stream, remoteEndPoint, null, options)
        {
            _stream = stream;
        }

        public QuicPipeConnection(QuicPipeStream stream, EndPoint remoteEndPoint, EndPoint localEndPoint,
            ConnectionOptions options)
            : base(stream, remoteEndPoint, localEndPoint, options)
        {
            _stream = stream;
        }

        protected override async Task StartInputPipeTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe,
            CancellationToken cancellationToken)
        {
            await _stream.OpenStreamAsync(cancellationToken);
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
}

#pragma warning disable CA2252