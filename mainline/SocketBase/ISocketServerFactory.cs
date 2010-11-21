using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    public interface ISocketServerFactory
    {
        ISocketServer CreateSocketServer<TAppSession, TCommandInfo>(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, IServerConfig config, object protocol)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : ICommandInfo;
    }
}
