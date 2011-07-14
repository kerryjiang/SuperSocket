using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandLoader
    {
        bool LoadCommands<TAppSession, TCommandInfo>(IAppServer appServer, Func<ICommand<TAppSession, TCommandInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TCommandInfo>>>> commandUpdater)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : ICommandInfo;
    }
}
