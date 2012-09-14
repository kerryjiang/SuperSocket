using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// GlobalPerformanceData class
    /// </summary>
    [Serializable]
    public class GlobalPerformanceData
    {
        /// <summary>
        /// Gets or sets the available working threads.
        /// </summary>
        /// <value>
        /// The available working threads.
        /// </value>
        public int AvailableWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the available completion port threads.
        /// </summary>
        /// <value>
        /// The available completion port threads.
        /// </value>
        public int AvailableCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets or sets the max working threads.
        /// </summary>
        /// <value>
        /// The max working threads.
        /// </value>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the max completion port threads.
        /// </summary>
        /// <value>
        /// The max completion port threads.
        /// </value>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets or sets the total thread count.
        /// </summary>
        /// <value>
        /// The total thread count.
        /// </value>
        public int TotalThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the cpu usage.
        /// </summary>
        /// <value>
        /// The cpu usage.
        /// </value>
        public double CpuUsage { get; set; }

        /// <summary>
        /// Gets or sets the working set.
        /// </summary>
        /// <value>
        /// The working set.
        /// </value>
        public long WorkingSet { get; set; }
    }
}
