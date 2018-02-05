using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Sockets;
using System.Net.Sockets;

namespace SuperSocket.NetSocket
{
    public class NetSocketPipeListener : IDuplexPipeListener
    {
        private SocketListener _socketListener;

        public void Start(IPEndPoint endpoint, Func<IDuplexPipe, Task> callback)
        {
            _socketListener = new SocketListener();
            _socketListener.Start(endpoint);
            _socketListener.OnConnection((c) => callback(c));
        }

        public void Stop()
        {
            _socketListener.Stop();
            _socketListener = null;
        }
    }
}