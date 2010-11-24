using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandLoader<TAppSession, TCommandInfo>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession, IAppSession<TCommandInfo>, new()
    {
        List<ICommand<TAppSession, TCommandInfo>> LoadCommands();
    }
}
