using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    public delegate void CommandHandler<TAppSession, TCommandInfo>(TAppSession session, TCommandInfo commandInfo)
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo;
}
