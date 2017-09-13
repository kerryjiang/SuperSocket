using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public abstract class SocketChannelBase : ChannelBase
    {
        public Socket Socket { get; private set; }

        public SocketChannelBase(Socket socket)
        {
            Socket = socket;
        }

        protected override void OnClosed()
        {
            Socket = null;
            base.OnClosed();
        }
    }
}
