using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command loader's interface
    /// </summary>
    public interface ICommandLoader
    {
        /// <summary>
        /// Initializes the command loader
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        bool Initialize<TCommand>(IAppServer appServer)
            where TCommand : ICommand;

        /// <summary>
        /// Tries to load commands.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        bool TryLoadCommands(out IEnumerable<ICommand> commands);

        /// <summary>
        /// Occurs when [updated].
        /// </summary>
        event EventHandler<CommandUpdateEventArgs<ICommand>> Updated;

        /// <summary>
        /// Occurs when [error].
        /// </summary>
        event EventHandler<ErrorEventArgs> Error;
    }
}
