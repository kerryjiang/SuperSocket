using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IRootConfig
    {
        /// <summary>
        /// Gets the logging mode.
        /// </summary>
        LoggingMode LoggingMode { get; }

        /// <summary>
        /// Gets the max working threads.
        /// </summary>
        int MaxWorkingThreads { get; }

        /// <summary>
        /// Gets the min working threads.
        /// </summary>
        int MinWorkingThreads { get; }

        /// <summary>
        /// Gets the max completion port threads.
        /// </summary>
        int MaxCompletionPortThreads { get; }

        /// <summary>
        /// Gets the min completion port threads.
        /// </summary>
        int MinCompletionPortThreads { get; }

        /// <summary>
        /// Gets a value indicating whether [disable performance data collector].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable performance data collector]; otherwise, <c>false</c>.
        /// </value>
        bool DisablePerformanceDataCollector { get; }

        /// <summary>
        /// Gets the performance data collect interval, in seconds.
        /// </summary>
        int PerformanceDataCollectInterval { get; }
    }
}
