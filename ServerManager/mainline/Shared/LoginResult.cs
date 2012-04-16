using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    /// <summary>
    /// Login command result
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoginResult"/> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.
        /// </value>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets the server's information.
        /// </summary>
        /// <value>
        /// The server's information.
        /// </value>
        public ServerInfo ServerInfo { get; set; }
    }
}
