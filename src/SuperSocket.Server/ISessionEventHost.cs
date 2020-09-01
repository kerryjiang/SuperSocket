using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public interface ISessionEventHost
    {
        ValueTask HandleSessionConnectedEvent(AppSession session);

        ValueTask HandleSessionClosedEvent(AppSession session);
    }
}