using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;
using System.Net.Sockets;
using System.Net;

namespace SuperSocket.Channel
{
    public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
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

        public override void Close()
        {
            _stream.Close();
        }


        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory)
        {
            return await _stream.ReadAsync(memory);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer)
        {
            var total = 0;

            foreach (var data in buffer)
            {
                await _stream.WriteAsync(data);
                total += data.Length;
            }

            await _stream.FlushAsync();
            return total;
        }

        protected override bool IsIgnorableException(Exception e)
        {
            if (e.InnerException != null)
                return IsIgnorableException(e.InnerException);

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }
    }
}