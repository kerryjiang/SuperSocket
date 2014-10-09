using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.OpHandlers
{
    interface IOpHandler
    {
        sbyte OpCode { get; }

        WebSocketPackageInfo Handle(IAppSession session, WebSocketContext context, IList<ArraySegment<byte>> data);
    }
}
