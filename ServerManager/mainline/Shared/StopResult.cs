using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    /// <summary>
    /// Stop command result
    /// </summary>
    public class StopResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StopResult"/> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.
        /// </value>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the server's information.
        /// </summary>
        /// <value>
        /// The server's information.
        /// </value>
        public ServerInfo ServerInfo { get; set; }
    }
}
