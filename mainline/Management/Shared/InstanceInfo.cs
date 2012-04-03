using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    /// <summary>
    /// AppServer instance's information
    /// </summary>
    public class InstanceInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the started time.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        public DateTime StartedTime { get; set; }

        /// <summary>
        /// Gets or sets the max connection count.
        /// </summary>
        /// <value>
        /// The max connection count.
        /// </value>
        public int MaxConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the current connection count.
        /// </summary>
        /// <value>
        /// The current connection count.
        /// </value>
        public int CurrentConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the listeners.
        /// </summary>
        /// <value>
        /// The listeners.
        /// </value>
        public ListenerInfo[] Listeners { get; set; }

        /// <summary>
        /// Gets or sets the request handling speed.
        /// </summary>
        /// <value>
        /// The request handling speed.
        /// </value>
        public int RequestHandlingSpeed { get; set; }
    }
}
