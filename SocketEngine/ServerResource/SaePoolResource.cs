using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.ServerResource;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using System.Security.Authentication;

namespace SuperSocket.SocketEngine.ServerResource
{
    class SaePoolResource : ServerResourceItem<IPool<SaeState>>
    {
        public SaePoolResource()
            : base("SaePool")
        {

        }

        protected override IPool<SaeState> CreateResource(IServerConfig config)
        {
            var server = AppContext.CurrentServer;

            //Plain tcp socket or udp
            if (config.Mode == SocketMode.Udp || server.Listeners.Any(l => l.Security == SslProtocols.None))
            {
                int bufferSize = config.ReceiveBufferSize;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4;
                
                var bufferManager = server.BufferManager;

                var initialCount = Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber);

                var socketServer = (server as ISocketServerAccessor).SocketServer;
                return new IntelliPool<SaeState>(initialCount, new SaeStateCreator(bufferManager, bufferSize, socketServer as IAsyncSocketEventComplete));
            }

            return null;
        }
    }
}
