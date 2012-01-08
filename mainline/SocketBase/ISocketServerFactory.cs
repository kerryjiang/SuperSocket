using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public interface ISocketServerFactory
    {
        ISocketServer CreateSocketServer<TAppSession, TRequestInfo>(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, IServerConfig config, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo;
    }
}
