using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    public abstract class StringCommandBase<TAppSession> : CommandBase<TAppSession, StringRequestInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringRequestInfo>, new()
    {

    }

    public abstract class StringCommandBase : StringCommandBase<AppSession>
    {

    }
}
