using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    public delegate void ExceptionCallback();

    public interface IAsyncRunner
    {

    }

    public static class AsyncRunnerExtension
    {
        public static void ExecuteAsync(this IAsyncRunner runner, string stepName, WaitCallback callback)
        {
            ExecuteAsync(runner, stepName, callback, null);
        }

        public static void ExecuteAsync(this IAsyncRunner runner, string stepName, WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    callback(null);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(stepName, e);
                    if (exceptionCallback != null)
                    {
                        try
                        {
                            exceptionCallback();
                        }
                        catch (Exception exc)
                        {
                            LogUtil.LogError(stepName + " exception callback", exc);
                        }
                    }
                }
            });
        }
    }
}
