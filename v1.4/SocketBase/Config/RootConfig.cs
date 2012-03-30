using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.SocketBase.Config
{
    public class RootConfig : IRootConfig
    {
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
        /// Gets the logging mode.
        /// </summary>
        public LoggingMode LoggingMode { get; set; }

        /// <summary>
        /// Gets the max working threads.
        /// </summary>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets the min working threads.
        /// </summary>
        public int MinWorkingThreads { get; set; }

        /// <summary>
        /// Gets the max completion port threads.
        /// </summary>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets the min completion port threads.
        /// </summary>
        public int MinCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets the performance data collect interval, in seconds.
        /// </summary>
        public int PerformanceDataCollectInterval { get; set; }

        #endregion
    }
}
