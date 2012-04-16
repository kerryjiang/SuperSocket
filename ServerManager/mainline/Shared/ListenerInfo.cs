using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SuperSocket.Management.Shared
{
    /// <summary>
    /// Listener's information
    /// </summary>
    public class ListenerInfo
    {
        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public string EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the back log.
        /// </summary>
        /// <value>
        /// The back log.
        /// </value>
        public int BackLog { get; set; }

        /// <summary>
        /// Gets or sets the security.
        /// </summary>
        /// <value>
        /// The security.
        /// </value>
        public string Security { get; set; }
    }
}
