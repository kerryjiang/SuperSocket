using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using Microsoft.Extensions.ObjectPool;

namespace SuperSocket.Connection
{
    public class TcpPipeConnection : PipeConnection
    {
        private Socket _socket;

        private readonly ObjectPool<SocketSender> _socketSenderPool;

        public TcpPipeConnection(Socket socket, ConnectionOptions options, ObjectPool<SocketSender> socketSenderPool = null)
            : base(options)
        {
            _socket = socket;
            RemoteEndPoint = socket.RemoteEndPoint;
            LocalEndPoint = socket.LocalEndPoint;

            _socketSenderPool = socketSenderPool;
        }

        protected override void OnClosed()
        {
            _socket = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await ReceiveAsync(_socket, memory, SocketFlags.None, cancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask<int> ReceiveAsync(Socket socket, Memory<byte> memory, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return await socket
                .ReceiveAsync(GetArrayByMemory(memory), socketFlags, cancellationToken)
                .ConfigureAwait(false);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var socketSenderPool = _socketSenderPool;

            var socketSender = socketSenderPool?.Get() ?? new SocketSender();

            try
            {
                var sentBytes = await socketSender.SendAsync(_socket, buffer).ConfigureAwait(false);

                if (socketSenderPool != null)
                {
                    socketSenderPool.Return(socketSender);
                    socketSender = null;
                }

                return sentBytes;
            }
            finally
            {
                socketSender?.Dispose();
            }
        }

        protected override void Close()
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
