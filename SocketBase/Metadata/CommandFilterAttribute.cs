using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// Command filter attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the execution order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Called when [command executing].
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract void OnCommandExecuting(CommandExecutingContext commandContext);

        /// <summary>
        /// Called when [command executed].
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract void OnCommandExecuted(CommandExecutingContext commandContext);
    }
}

