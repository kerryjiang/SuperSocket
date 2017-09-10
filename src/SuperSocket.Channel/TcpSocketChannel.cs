using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public class TcpSocketChannel : SocketChannelBase
    {
        public TcpSocketChannel(Socket socket)
            : base(socket)
        {
            
        }

        public override Task<ArraySegment<byte>> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public override Task SendAsync(ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
