using System;

namespace SuperSocket.Command
{
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
    }
}
