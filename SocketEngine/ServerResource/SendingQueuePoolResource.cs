using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.ServerResource;
using SuperSocket.SocketBase.Utils;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.ServerResource
{
    class SendingQueuePoolResource : ServerResourceItem<IPool<SendingQueue>>
    {
        public SendingQueuePoolResource()
            : base("SendingQueuePool")
        {

        }

        protected override IPool<SendingQueue> CreateResource(IServerConfig config)
        {
            return new IntelliPool<SendingQueue>(Math.Max(config.MaxConnectionNumber / 6, 256), new SendingQueueSourceCreator(config.SendingQueueSize));
        }
    }
}
