using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public interface ISessionEventHost
    {
        ValueTask HandleSessionConnectedEvent(AppSession session);

        ValueTask HandleSessionClosedEvent(AppSession session, CloseReason reason);
    }
}