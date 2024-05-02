using System;
using System.Buffers;
using System.Net.Quic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

#pragma warning disable CA2252

namespace SuperSocket.Quic.Connection
{
    public class QuicPipeConnection : PipeConnection
    {
        private QuicStream _stream;
        private ValueTask<QuicStream> _readStreamTask;
        private readonly QuicConnection _quicConnection;

        public QuicPipeConnection(QuicConnection quicConnection, ConnectionOptions options) : base(options)
        {
            _quicConnection = quicConnection;
            RemoteEndPoint = quicConnection.RemoteEndPoint;
            LocalEndPoint = quicConnection.LocalEndPoint;
        }

        protected override async Task StartInputPipeTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_readStreamTask);

            _stream = await _readStreamTask.ConfigureAwait(false);

            await base.StartInputPipeTask(packagePipe, cancellationToken).ConfigureAwait(false);
        }

        public void OpenOutboundStream(QuicStreamType quicStreamType, CancellationToken cancellationToken)
        {
            _readStreamTask = _quicConnection.OpenOutboundStreamAsync(quicStreamType, cancellationToken);
        }

        public void AcceptInboundStream(CancellationToken cancellationToken)
        {
            _readStreamTask = _quicConnection.AcceptInboundStreamAsync(cancellationToken);
        }

        protected override void Close()
        {
            ThrowIfStreamNull();
            
            _stream.Close();
        }

        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory,
            CancellationToken cancellationToken)
        {
            ThrowIfStreamNull();
            
            return await _stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer,
            CancellationToken cancellationToken)
        {
            ThrowIfStreamNull();
            
            var total = 0;

            foreach (var data in buffer)
            {
                await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                total += data.Length;
            }

            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return total;
        }

        protected override bool IsIgnorableException(Exception e)
        {
            if (base.IsIgnorableException(e))
                return true;

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }

        private void ThrowIfStreamNull()
        {
            ArgumentNullException.ThrowIfNull(_stream);
        }
    }
}

#pragma warning disable CA2252