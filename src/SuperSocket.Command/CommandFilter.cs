using System;

namespace SuperSocket.Command
{
    public interface ICommandFilter
    {
        int Order { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterBaseAttribute : Attribute, ICommandFilter
    {
        /// <summary>
        /// Gets or sets the execution order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called when [command executing].
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract bool OnCommandExecuting(CommandExecutingContext commandContext);

        /// <summary>
        /// Called when [command executed].
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        public abstract void OnCommandExecuted(CommandExecutingContext commandContext);
    }
}
