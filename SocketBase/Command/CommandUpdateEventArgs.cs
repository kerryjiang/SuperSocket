using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// CommandUpdateEventArgs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandUpdateEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the commands updated.
        /// </summary>
        public IEnumerable<CommandUpdateInfo<T>> Commands { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandUpdateEventArgs&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="commands">The commands.</param>
        public CommandUpdateEventArgs(IEnumerable<CommandUpdateInfo<T>> commands)
        {
            Commands = commands;
        }
    }
}
