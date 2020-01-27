using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AsyncCommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called when [command executing].
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns>return if the service should continue to process this session</returns>
        public abstract ValueTask<bool> OnCommandExecutingAsync(CommandExecutingContext commandContext);

        /// <summary>
        /// Called when [command executed].
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract ValueTask OnCommandExecutedAsync(CommandExecutingContext commandContext);
    }
}
