using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Sockets;
using System.Net.Sockets;

namespace SuperSocket.NetSocket
{
    public class NetSocketPipeConnectionListener : IPipeConnectionListener
    {
        private SocketListener _socketListener;
        public void Start(IPEndPoint endpoint, Func<IPipeConnection, Task> callback)
        {
            _socketListener = new SocketListener();
            _socketListener.Start(endpoint);
            _socketListener.ListeningSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _socketListener.OnConnection((c) => callback(c));
        }

        public void Stop()
        {
            _socketListener.Stop();
            _socketListener = null;
        }
    }
}