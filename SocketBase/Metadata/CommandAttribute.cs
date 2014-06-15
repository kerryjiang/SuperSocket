using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// The command metadata attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the key of this command
        /// </summary>
        /// <value>
        /// The command key.
        /// </value>
        public object Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public CommandAttribute(object key)
        {
            Key = key;
        }
    }
}
