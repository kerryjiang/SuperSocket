using System;

namespace SuperSocket.Command
{
    /// <summary>
    /// Defines a filter that can be applied to commands.
    /// </summary>
    public interface ICommandFilter
    {
        /// <summary>
        /// Gets the execution order of the filter.
        /// </summary>
        int Order { get; }
    }

    /// <summary>
    /// Represents a base attribute for command filters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterBaseAttribute : Attribute, ICommandFilter
    {
        /// <summary>
        /// Gets or sets the execution order of the filter.
        /// </summary>
        public int Order { get; set; }
    }

    /// <summary>
    /// Represents an attribute for filtering commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : CommandFilterBaseAttribute
    {
        /// <summary>
        /// Called before a command is executed.
        /// </summary>
        /// <param name="commandContext">The context of the command being executed.</param>
        /// <returns>True if the command should continue execution; otherwise, false.</returns>
        public abstract bool OnCommandExecuting(CommandExecutingContext commandContext);

        /// <summary>
        /// Called after a command is executed.
        /// </summary>
        /// <param name="commandContext">The context of the command that was executed.</param>
        public abstract void OnCommandExecuted(CommandExecutingContext commandContext);
    }
}
