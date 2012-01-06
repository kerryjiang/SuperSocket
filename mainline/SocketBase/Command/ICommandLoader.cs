using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandLoader
    {
        bool LoadCommands<TAppSession, TRequestInfo>(IAppServer appServer, Func<ICommand<TAppSession, TRequestInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>>> commandUpdater)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo;
    }
}
