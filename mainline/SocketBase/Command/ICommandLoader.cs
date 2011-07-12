using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandLoader
    {
        IEnumerable<ICommand<TAppSession, TCommandInfo>> LoadCommands<TAppSession, TCommandInfo>(IAppServer appServer)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : ICommandInfo;
    }
}
