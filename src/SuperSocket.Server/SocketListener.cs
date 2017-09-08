using System;
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
        }

        public void StartListen()
        {
            var socket = new Socket(Listener.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                socket.Bind(Listener.EndPoint);
                socket.Listen(Listener.BackLog);
                _listenSocket = socket;
            }
            catch (Exception e)
            {
                socket.Dispose();
                throw e;
            }
        }

        public async Task<Socket> AcceptAsync()
        {
            return await _listenSocket.AcceptAsync();
        }
    }
}