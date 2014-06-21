using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.ServerResource;

namespace SuperSocket.SocketEngine.ServerResource
{
    class BufferStatePoolResource : ServerResourceItem<IPool<BufferState>>
    {
        public BufferStatePoolResource()
            : base("BufferStatePool")
        {

        }

        protected override IPool<BufferState> CreateResource(IServerConfig config)
        {
            var server = AppContext.CurrentServer;

            //No TLS/SSL TCP listener
            if (!server.Listeners.Any(l => l.Security != SslProtocols.None))
                return null;

            int bufferSize = config.ReceiveBufferSize;

            if (bufferSize <= 0)
                bufferSize = 1024 * 4;

            var bufferManager = server.BufferManager;

            var initialCount = Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber);

            IPool<BufferState> pool = null;
            pool = new IntelliPool<BufferState>(initialCount, new BufferStateCreator(pool, bufferManager, bufferSize));

            return pool;
        }
    }
}
