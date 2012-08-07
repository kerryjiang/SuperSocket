using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command update action enum
    /// </summary>
    public enum CommandUpdateAction
    {
        /// <summary>
        /// Add command
        /// </summary>
        Add,

        /// <summary>
        /// Remove command
        /// </summary>
        Remove,

        /// <summary>
        /// Update command
        /// </summary>
        Update
    }

    /// <summary>
    /// Command update information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandUpdateInfo<T>
    {
        /// <summary>
        /// Gets or sets the update action.
        /// </summary>
        /// <value>
        /// The update action.
        /// </value>
        public CommandUpdateAction UpdateAction { get; set; }

        /// <summary>
        /// Gets or sets the target command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public T Command { get; set; }
    }
}