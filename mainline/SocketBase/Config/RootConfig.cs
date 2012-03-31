using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Root configuration model
    /// </summary>
    public class RootConfig : IRootConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootConfig"/> class.
        /// </summary>
        public RootConfig()
        {
            int maxWorkingThread, maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThread, out maxCompletionPortThreads);
            MaxWorkingThreads = maxWorkingThread;
            MaxCompletionPortThreads = maxCompletionPortThreads;

            int minWorkingThread, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkingThread, out minCompletionPortThreads);
            MinWorkingThreads = minWorkingThread;
            MinCompletionPortThreads = minCompletionPortThreads;

            PerformanceDataCollectInterval = 60;
        }

        #region IRootConfig Members

        /// <summary>
        /// Gets/Sets the max working threads.
        /// </summary>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the min working threads.
        /// </summary>
        public int MinWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the max completion port threads.
        /// </summary>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets/sets the min completion port threads.
        /// </summary>
        public int MinCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets/sets the performance data collect interval, in seconds.
        /// </summary>
        public int PerformanceDataCollectInterval { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether [disable performance data collector].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable performance data collector]; otherwise, <c>false</c>.
        /// </value>
        public bool DisablePerformanceDataCollector { get; set; }

        #endregion
    }
}
