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
        }

        #region IRootConfig Members

        public int MaxWorkingThreads { get; set; }

        public int MinWorkingThreads { get; set; }

        public int MaxCompletionPortThreads { get; set; }

        public int MinCompletionPortThreads { get; set; }

        #endregion
    }
}
