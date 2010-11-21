using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    public interface ICommandSource<TCommand> where TCommand : ICommand
    {
        TCommand GetCommandByName(string commandName);
    }
}
