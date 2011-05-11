using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class StringCommandBase<TAppSession> : CommandBase<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {

    }

    public abstract class StringCommandBase : StringCommandBase<AppSession>
    {

    }
}
