using System;
using System.IO.Pipelines;
using System.IO.Pipelines.Networking.Libuv;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket.Libuv
{
    public class LibuvPipeConnectionListener : IPipeConnectionListener
    {
        private UvTcpListener _listener;
        private UvThread _uvThread;

        public LibuvPipeConnectionListener()
        {
        }

        public void Start(IPEndPoint endpoint, Func<IPipeConnection, Task> callback)
        {
            _uvThread = new UvThread();
            _listener = new UvTcpListener(_uvThread, endpoint);
            _listener.OnConnection((c) => callback(c));
            _listener.StartAsync().GetAwaiter().GetResult();
        }

        public void Stop()
        {
            _listener.Dispose();
            _listener = null;
            _uvThread = null;
        }
    }
}