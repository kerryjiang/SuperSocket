using System;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class TcpPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
    {

        private Socket _socket;

        private List<ArraySegment<byte>> _segmentsForSend;
        
        public TcpPipeChannel(Socket socket, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _socket = socket;
        }

        protected override void OnClosed()
        {
            _socket = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory)
        {
            return await ReceiveAsync(_socket, memory, SocketFlags.None);
        }

        private async Task<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags)
        {
            return await socket.ReceiveAsync(GetArrayByMemory((ReadOnlyMemory<byte>)memory), socketFlags);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return await _socket.SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None);
            }
            
            if (_segmentsForSend == null)
            {
                _segmentsForSend = new List<ArraySegment<byte>>();
            }
            else
            {
                _segmentsForSend.Clear();
            }

            var segments = _segmentsForSend;

            foreach (var piece in buffer)
            {
                _segmentsForSend.Add(GetArrayByMemory(piece));
            }
            
            return await _socket.SendAsync(_segmentsForSend, SocketFlags.None);
        }

        public override void Close()
        {
            var socket = _socket;

            if (socket == null)
                return;

            if (Interlocked.CompareExchange(ref _socket, null, socket) == socket)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    socket.Close();
                }
            }
        }

        protected override bool IsIgnorableException(Exception e)
        {
            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }
    }
}
