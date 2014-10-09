using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.OpHandlers
{
    abstract class OpHandlerBase : IOpHandler
    {
        protected OpHandlerBase(WebSocketServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected WebSocketServiceProvider ServiceProvider { get; private set; }

        public abstract sbyte OpCode { get; }

        public abstract WebSocketPackageInfo Handle(IAppSession session, WebSocketContext context, IList<ArraySegment<byte>> data);
    }
}
