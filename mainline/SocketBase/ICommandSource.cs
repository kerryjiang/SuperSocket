using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for class who provides commands source
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public interface ICommandSource<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Gets the command by it's name.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <returns></returns>
        TCommand GetCommandByName(string commandName);
    }
}
