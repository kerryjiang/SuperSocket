using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    public static class TheadPoolEx
    {
        public static bool ResetThreadPool(int? maxWorkingThreads, int? maxCompletionPortThreads, int? minWorkingThreads, int? minCompletionPortThreads)
        {
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
                        LogUtil.LogError(string.Format("Failed to run ThreadPool.SetMaxThreads({0}, {1})", maxWorkingThreads.Value, maxCompletionPortThreads.Value));
                        return false;
                    }
                    else
                    {
                        LogUtil.LogInfo(string.Format("ThreadPool.SetMaxThreads({0}, {1})", maxWorkingThreads.Value, maxCompletionPortThreads.Value));
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
                        LogUtil.LogError(string.Format("Failed to run ThreadPool.SetMinThreads({0}, {1})", minWorkingThreads.Value, minCompletionPortThreads.Value));
                        return false;
                    }
                    else
                    {
                        LogUtil.LogInfo(string.Format("ThreadPool.SetMinThreads({0}, {1})", minWorkingThreads.Value, minCompletionPortThreads.Value));
                    }
                }
            }

            return true;
        }
    }
}
