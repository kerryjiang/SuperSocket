using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// PerformanceDataInfo class, which assosiates server with performance data
    /// </summary>
    public class PerformanceDataInfo
    {
        /// <summary>
        /// Gets or sets the name of the server instance.
        /// </summary>
        /// <value>
        /// The name of the server.
        /// </value>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the performance data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public PerformanceData Data { get; set; }
    }
}
