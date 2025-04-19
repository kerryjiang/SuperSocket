using System;
using System.Threading;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents the context for executing a command.
    /// </summary>
    public struct CommandExecutingContext
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        public IAppSession Session { get; set; }

        /// <summary>
        /// Gets the request info.
        /// </summary>
        public object Package { get; set; }

        /// <summary>
        /// Gets the current command.
        /// </summary>
        public ICommand CurrentCommand { get; set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
