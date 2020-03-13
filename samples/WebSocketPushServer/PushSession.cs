using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server;
using SuperSocket.SessionContainer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace WebSocketPushServer
{
    public class PushSession : WebSocketSession
    {
        protected override async ValueTask OnSessionConnectedAsync()
        {
            await this.SendAsync(this.SessionID);
        }
    }
}
