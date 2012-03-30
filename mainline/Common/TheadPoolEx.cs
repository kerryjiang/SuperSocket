using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common.Logging;

namespace SuperSocket.Common
{
    /// <summary>
    /// Thread pool extension class
    /// </summary>
    public static class TheadPoolEx
    {
        /// <summary>
        /// Resets the thread pool.
        /// </summary>
        /// <param name="maxWorkingThreads">The max working threads.</param>
        /// <param name="maxCompletionPortThreads">The max completion port threads.</param>
        /// <param name="minWorkingThreads">The min working threads.</param>
        /// <param name="minCompletionPortThreads">The min completion port threads.</param>
        /// <returns></returns>
        public static bool ResetThreadPool(int? maxWorkingThreads, int? maxCompletionPortThreads, int? minWorkingThreads, int? minCompletionPortThreads)
        {
            var log = LogFactoryProvider.GlobalLog;

            if (maxWorkingThreads.HasValue || maxCompletionPortThreads.HasValue)
            {
                int oldMaxWorkingThreads, oldMaxCompletionPortThreads;

                ThreadPool.GetMaxThreads(out oldMaxWorkingThreads, out oldMaxCompletionPortThreads);

                if (!maxWorkingThreads.HasValue)
                    maxWorkingThreads = oldMaxWorkingThreads;

                if (!maxCompletionPortThreads.HasValue)
                    maxCompletionPortThreads = oldMaxCompletionPortThreads;

                if (maxWorkingThreads.Value != oldMaxWorkingThreads
                    || maxCompletionPortThreads.Value != oldMaxCompletionPortThreads)
                {
                    if (!ThreadPool.SetMaxThreads(maxWorkingThreads.Value, maxCompletionPortThreads.Value))
                    {
                        if (log.IsErrorEnabled)
                            log.ErrorFormat("Failed to run ThreadPool.SetMaxThreads({0}, {1})", maxWorkingThreads.Value, maxCompletionPortThreads.Value);
                        return false;
                    }
                    else
                    {
                        if (log.IsInfoEnabled)
                            log.InfoFormat("ThreadPool.SetMaxThreads({0}, {1})", maxWorkingThreads.Value, maxCompletionPortThreads.Value);
                    }
                }
            }

            if (minWorkingThreads.HasValue || minCompletionPortThreads.HasValue)
            {
                int oldMinWorkingThreads, oldMinCompletionPortThreads;

                ThreadPool.GetMinThreads(out oldMinWorkingThreads, out oldMinCompletionPortThreads);

                if (!minWorkingThreads.HasValue)
                    minWorkingThreads = oldMinWorkingThreads;

                if (!minCompletionPortThreads.HasValue)
                    minCompletionPortThreads = oldMinCompletionPortThreads;

                if (minWorkingThreads.Value != oldMinWorkingThreads
                    || minCompletionPortThreads.Value != oldMinCompletionPortThreads)
                {
                    if (!ThreadPool.SetMinThreads(minWorkingThreads.Value, minCompletionPortThreads.Value))
                    {
                        if (log.IsErrorEnabled)
                            log.ErrorFormat("Failed to run ThreadPool.SetMinThreads({0}, {1})", minWorkingThreads.Value, minCompletionPortThreads.Value);
                        return false;
                    }
                    else
                    {
                        if (log.IsInfoEnabled)
                            log.InfoFormat("ThreadPool.SetMinThreads({0}, {1})", minWorkingThreads.Value, minCompletionPortThreads.Value);
                    }
                }
            }

            return true;
        }
    }
}
