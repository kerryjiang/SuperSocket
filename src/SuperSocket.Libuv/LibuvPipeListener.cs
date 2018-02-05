using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Libuv;

namespace SuperSocket.Libuv
{
    public class LibuvPipeListener : IDuplexPipeListener
    {
        private UvTcpListener _listener;
        private UvThread _uvThread;

        public LibuvPipeListener()
        {
            
        }

        public void Start(IPEndPoint endpoint, Func<IDuplexPipe, Task> callback)
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