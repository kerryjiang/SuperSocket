using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents an attribute for filtering asynchronous commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AsyncCommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called before a command is executed asynchronously.
        /// </summary>
        /// <param name="commandContext">The context of the command being executed.</param>
        /// <returns>A task that represents the asynchronous operation. Returns true if the command should continue execution; otherwise, false.</returns>
        public abstract ValueTask<bool> OnCommandExecutingAsync(CommandExecutingContext commandContext);

        /// <summary>
        /// Called after a command is executed asynchronously.
        /// </summary>
        /// <param name="commandContext">The context of the command that was executed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract ValueTask OnCommandExecutedAsync(CommandExecutingContext commandContext);
    }
}
