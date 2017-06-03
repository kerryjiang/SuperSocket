using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    class SocketListener
    {
        internal Listener Listener { get; private set;}

        private Socket _listenSocket;

        public SocketListener(Listener listener)
        {
            Listener = listener;

            var socket = _listenSocket = new Socket(Listener.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            socket.Bind(Listener.EndPoint);
            socket.Listen(100);
        }

        public async Task<Socket> AcceptAsync()
        {
            return await _listenSocket.AcceptAsync();
        }
    }
}