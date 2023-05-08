using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class UdpPipeChannel<TPackageInfo> : VirtualChannel<TPackageInfo>, IChannelWithSessionIdentifier
    {
        private Socket _socket;

        private bool _enableSendingPipe;

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint)
            : this(socket, pipelineFilter, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
        {

        }

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
            : base(pipelineFilter, options)
        {
            _socket = socket;            
            _enableSendingPipe = "true".Equals(options.Values?["enableSendingPipe"], StringComparison.OrdinalIgnoreCase);
            RemoteEndPoint = remoteEndPoint;
            SessionIdentifier = sessionIdentifier;
        }

        public string SessionIdentifier { get; }

        protected override void Close()
        {
            WriteEOFPackage();
        }

        protected override ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe || buffer.IsSingleSegment)
            {
                var total = 0;

                foreach (var piece in buffer)
                {
                    total += await _socket.SendToAsync(GetArrayByMemory<byte>(piece), SocketFlags.None, RemoteEndPoint);
                }

                return total;
            }

            var pool = ArrayPool<byte>.Shared;
            var destBuffer = pool.Rent((int)buffer.Length);

            try
            {                
                MergeBuffer(ref buffer, destBuffer);
                return await _socket.SendToAsync(new ArraySegment<byte>(destBuffer, 0, (int)buffer.Length), SocketFlags.None, RemoteEndPoint);       
            }
            finally
            {
                pool.Return(destBuffer);
            }            
        }

        protected override Task ProcessSends()
        {
            if (_enableSendingPipe)
                return base.ProcessSends();

            return Task.CompletedTask;
        }

        private void MergeBuffer(ref ReadOnlySequence<byte> buffer, byte[] destBuffer)
        {
            Span<byte> destSpan = destBuffer;

            var total = 0;

            foreach (var piece in buffer)
            {
                piece.Span.CopyTo(destSpan);
                total += piece.Length;
                destSpan = destSpan.Slice(piece.Length);                
            }
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            if (_enableSendingPipe)
            {
                await base.SendAsync(buffer);
                return;
            }

            await SendOverIOAsync(new ReadOnlySequence<byte>(buffer), CancellationToken.None);
        }
        
        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            if (_enableSendingPipe)
            {
                await base.SendAsync(packageEncoder, package);
                return;
            }

            try
            {
                await SendLock.WaitAsync();
                var writer = Out.Writer;
                WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);
                await writer.FlushAsync();
                await ProcessOutputRead(Out.Reader);
            }
            finally
            {
                SendLock.Release();
            }
        }

        public override async ValueTask SendAsync(Action<PipeWriter> write)
        {
            if (_enableSendingPipe)
            {
                await base.SendAsync(write);
                return;
            }

            throw new NotSupportedException($"The method SendAsync(Action<PipeWriter> write) cannot be used when noSendingPipe is true.");
        }
    }
}