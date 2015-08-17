using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    interface IWebSocketReceiveFilter : IReceiveFilter<WebSocketPackageInfo>, IHandshakeHandler
    {

    }
}
