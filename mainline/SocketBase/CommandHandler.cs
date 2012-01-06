using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    public delegate void CommandHandler<TAppSession, TRequestInfo>(TAppSession session, TRequestInfo requestInfo)
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo;
}
