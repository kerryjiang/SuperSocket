using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Command filter attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CommandFilterAttribute : Attribute
    {
        /// <summary>
        /// Called when [command executing].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="command">The command.</param>
        public abstract void OnCommandExecuting(IAppSession session, ICommand command);

        /// <summary>
        /// Called when [command executed].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="command">The command.</param>
        public abstract void OnCommandExecuted(IAppSession session, ICommand command);
    }
}

