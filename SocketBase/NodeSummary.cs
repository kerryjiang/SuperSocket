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
    public class NodeSummary
    {
        /// <summary>
        /// Gets or sets the cpu usage.
        /// </summary>
        /// <value>
        /// The cpu usage.
        /// </value>
        [Display(Name = "CPU Usage", Format = "{0:0.00}%", Order = 0)]
        public double CpuUsage { get; set; }

        /// <summary>
        /// Gets or sets the working set.
        /// </summary>
        /// <value>
        /// The working set.
        /// </value>
        [Display(Name = "Physical Memory Usage", Format = "{0:N}", Order = 1)]
        public long WorkingSet { get; set; }


        /// <summary>
        /// Gets or sets the total thread count.
        /// </summary>
        /// <value>
        /// The total thread count.
        /// </value>
        [Display(Name = "Total Thread Count", Order = 2)]
        public int TotalThreadCount { get; set; }
        /// <summary>
        /// Gets or sets the available working threads.
        /// </summary>
        /// <value>
        /// The available working threads.
        /// </value>
        [Display(Name = "Available Working Threads", Order = 3)]
        public int AvailableWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the available completion port threads.
        /// </summary>
        /// <value>
        /// The available completion port threads.
        /// </value>
        [Display(Name = "Available Completion Port Threads", Order = 4)]
        public int AvailableCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets or sets the max working threads.
        /// </summary>
        /// <value>
        /// The max working threads.
        /// </value>
        [Display(Name = "Maximum Working Threads", Order = 5)]
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the max completion port threads.
        /// </summary>
        /// <value>
        /// The max completion port threads.
        /// </value>
        [Display(Name = "Maximum Completion Port Threads", Order = 6)]
        public int MaxCompletionPortThreads { get; set; }
    }
}
