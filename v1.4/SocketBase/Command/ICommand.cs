using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TAppSession, TCommandInfo> : ICommand
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TCommandInfo>
    {
        void ExecuteCommand(TAppSession session, TCommandInfo commandInfo);
    }
}