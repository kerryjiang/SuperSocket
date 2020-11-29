using System;
using System.Buffers;
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

        private IPEndPoint _remoteEndPoint;

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint)
            : this(socket, pipelineFilter, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
        {

        }

        public UdpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
            : base(pipelineFilter, options)
        {
            _socket = socket;
            _remoteEndPoint = remoteEndPoint;
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
            var total = 0;

            foreach (var piece in buffer)
            {
                total += await _socket.SendToAsync(GetArrayByMemory<byte>(piece), SocketFlags.None, _remoteEndPoint);
            }

            return total;
        }
    }
}