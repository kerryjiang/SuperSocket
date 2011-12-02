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

    public interface ICommand<TCommandInfo, TContext> : ICommand
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        void ExecuteCommand(IClientSession<TCommandInfo, TContext> session, TCommandInfo commandInfo);
    }

    public interface ICommand<TSession, TCommandInfo, TContext> : ICommand
        where TSession : IClientSession<TCommandInfo, TContext>
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        void ExecuteCommand(TSession session, TCommandInfo commandInfo);
    }
}
