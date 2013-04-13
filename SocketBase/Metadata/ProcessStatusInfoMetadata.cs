using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// ProcessStatusInfoMetadata
    /// </summary>
    public class ProcessStatusInfoMetadata
    {
        /// <summary>
        /// Gets or sets the cpu usage.
        /// </summary>
        /// <value>
        /// The cpu usage.
        /// </value>
        [StatusInfo("CPU Usage", Format = "{0:0.00}%", Order = 0, DataType = typeof(double))]
        public const string CpuUsage = "CpuUsage";

        /// <summary>
        /// Gets or sets the working set.
        /// </summary>
        /// <value>
        /// The working set.
        /// </value>
        [StatusInfo("Physical Memory Usage", Format = "{0:N}", Order = 1, DataType = typeof(long))]
        public const string WorkingSet = "WorkingSet";


        /// <summary>
        /// Gets or sets the total thread count.
        /// </summary>
        /// <value>
        /// The total thread count.
        /// </value>
        [StatusInfo("Total Thread Count", Order = 2, DataType = typeof(int))]
        public const string TotalThreadCount = "TotalThreadCount";
        /// <summary>
        /// Gets or sets the available working threads.
        /// </summary>
        /// <value>
        /// The available working threads.
        /// </value>
        [StatusInfo("Available Working Threads", Order = 3, DataType = typeof(int))]
        public const string AvailableWorkingThreads = "AvailableWorkingThreads";

        /// <summary>
        /// Gets or sets the available completion port threads.
        /// </summary>
        /// <value>
        /// The available completion port threads.
        /// </value>
        [StatusInfo("Available Completion Port Threads", Order = 4, DataType = typeof(int))]
        public const string AvailableCompletionPortThreads = "AvailableCompletionPortThreads";

        /// <summary>
        /// Gets or sets the max working threads.
        /// </summary>
        /// <value>
        /// The max working threads.
        /// </value>
        [StatusInfo("Maximum Working Threads", Order = 5, DataType = typeof(int))]
        public const string MaxWorkingThreads = "MaxWorkingThreads";

        /// <summary>
        /// Gets or sets the max completion port threads.
        /// </summary>
        /// <value>
        /// The max completion port threads.
        /// </value>
        [StatusInfo("Maximum Completion Port Threads", Order = 6, DataType = typeof(int))]
        public const string MaxCompletionPortThreads = "MaxCompletionPortThreads";
    }
}
