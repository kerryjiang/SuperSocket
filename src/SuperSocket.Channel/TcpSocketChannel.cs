using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public class TcpSocketChannel : SocketChannelBase
    {

        public SocketState State { get; private set; }

        public bool InSending { get; private set; }

        public bool InReceiving { get; private set; }

        public TcpSocketChannel(Socket socket)
            : base(socket)
        {
            State = SocketState.Connected;
        }

        public override Task<ArraySegment<byte>> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public override Task SendAsync(ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            var socket =  Socket;

            if (socket == null || State != SocketState.Connected)
                return;

            socket.Shutdown(SocketShutdown.Both);
            socket.Dispose();

            State = SocketState.Closing;

            if (!InSending && !InReceiving)
            {
                OnClosed();
                State = SocketState.Closed;
            }            
        }
    }
}
