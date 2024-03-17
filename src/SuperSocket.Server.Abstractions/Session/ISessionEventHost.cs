using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Session
{
    public interface ISessionEventHost
    {
        ValueTask HandleSessionConnectedEvent(IAppSession session);

        ValueTask HandleSessionClosedEvent(IAppSession session, CloseReason reason);
    }
}