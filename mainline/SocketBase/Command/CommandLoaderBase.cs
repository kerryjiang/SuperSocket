using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// CommandLoader base class
    /// </summary>
    public abstract class CommandLoaderBase : ICommandLoader
    {
        /// <summary>
        /// Initializes the command loader
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public abstract bool Initialize<TCommand>(IRootConfig rootConfig, IAppServer appServer) where TCommand : ICommand;

        /// <summary>
        /// Tries to load commands.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public abstract bool TryLoadCommands(out IEnumerable<ICommand> commands);

        /// <summary>
        /// Called when [updated].
        /// </summary>
        /// <param name="commands">The commands.</param>
        protected void OnUpdated(IEnumerable<CommandUpdateInfo<ICommand>> commands)
        {
            var handler = Updated;

            if (handler != null)
                handler(this, new CommandUpdateEventArgs<ICommand>(commands));
        }

        /// <summary>
        /// Occurs when [updated].
        /// </summary>
        public event EventHandler<CommandUpdateEventArgs<ICommand>> Updated;

        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnError(string message)
        {
            OnError(new Exception(message));
        }

        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="e">The e.</param>
        protected void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, new ErrorEventArgs(e));
        }

        /// <summary>
        /// Occurs when [error].
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;
    }
}
