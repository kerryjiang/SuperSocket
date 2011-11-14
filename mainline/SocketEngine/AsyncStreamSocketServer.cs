using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketServer<TAppSession, TCommandInfo> : TcpSocketServerBase<AsyncStreamSocketSession<TAppSession, TCommandInfo>, TAppSession>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public AsyncStreamSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint)
        {
            
        }
    }
}
