using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.Management.Server
{
    /// <summary>
    /// AppServer's instance
    /// </summary>
    public class InstanceState
    {
        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public IAppServer Instance { get; set; }

        /// <summary>
        /// Gets or sets the performance stat.
        /// </summary>
        /// <value>
        /// The performance.
        /// </value>
        public PerformanceData Performance { get; set; }
    }
}
