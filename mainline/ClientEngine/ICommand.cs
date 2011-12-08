using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TSession, TCommandInfo> : ICommand
        where TSession : IClientSession
        where TCommandInfo : ICommandInfo
    {
        void ExecuteCommand(TSession session, TCommandInfo commandInfo);
    }
}
