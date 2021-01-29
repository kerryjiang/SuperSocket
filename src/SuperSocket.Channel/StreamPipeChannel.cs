using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
    {
        private Stream _stream;

        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : this(stream, remoteEndPoint, null, pipelineFilter, options)
        {
            
        }

        public StreamPipeChannel(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _stream = stream;
            RemoteEndPoint = remoteEndPoint;
            LocalEndPoint = localEndPoint;
        }

        protected override void Close()
        {
            _stream.Close();
        }


        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await _stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
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
    }
}