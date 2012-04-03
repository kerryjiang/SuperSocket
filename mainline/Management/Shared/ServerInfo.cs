using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    /// <summary>
    /// Server's information
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// Gets or sets the cpu usage.
        /// </summary>
        /// <value>
        /// The cpu usage.
        /// </value>
        public double CpuUsage { get; set; }

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
        /// Gets or sets the physical memory usage.
        /// </summary>
        /// <value>
        /// The physical memory usage.
        /// </value>
        public double PhysicalMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the total thread count.
        /// </summary>
        /// <value>
        /// The total thread count.
        /// </value>
        public int TotalThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the instances.
        /// </summary>
        /// <value>
        /// The instances.
        /// </value>
        public InstanceInfo[] Instances { get; set; }
    }
}
