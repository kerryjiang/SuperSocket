using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    public delegate void ExceptionCallback(Exception e);

    public enum AsyncRunType
    {
        NewThread,
        ThreadPool
    }

    public interface IAsyncRunner
    {

    }

    public static class AsyncRunnerExtension
    {
        class ExecuteAsyncThreadStartParameter
        {
            public WaitCallback RunCallback { get; set; }
            public ExceptionCallback ExceptionCallback { get; set; }
        }

        public static void ExecuteAsync(this IAsyncRunner runner, AsyncRunType runType, WaitCallback callback)
        {
            ExecuteAsync(runner, runType, callback, null);
        }

        public static void ExecuteAsync(this IAsyncRunner runner, WaitCallback callback)
        {
            ExecuteAsync(runner, AsyncRunType.ThreadPool, callback, null);
        }

        public static void ExecuteAsync(this IAsyncRunner runner, WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            runner.ExecuteAsync(AsyncRunType.ThreadPool, callback, exceptionCallback);
        }

        public static void ExecuteAsync(this IAsyncRunner runner, AsyncRunType runType, WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            if (runType == AsyncRunType.ThreadPool)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ExecuteAsyncInternal(callback, exceptionCallback);
                });
            }
            else
            {
                Thread newThread = new Thread(new ParameterizedThreadStart(ExecuteAsyncStart));
                newThread.Start(new ExecuteAsyncThreadStartParameter
                    {
                        RunCallback = callback,
                        ExceptionCallback = exceptionCallback
                    });

            }
        }

        private static void ExecuteAsyncStart(object parameter)
        {
            ExecuteAsyncThreadStartParameter startParameter = parameter as ExecuteAsyncThreadStartParameter;

            if (startParameter == null)
                return;

            ExecuteAsyncInternal(startParameter.RunCallback, startParameter.ExceptionCallback);
        }

        private static void ExecuteAsyncInternal(WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            try
            {
                callback(null);
            }
            catch (Exception e)
            {
                if (exceptionCallback != null)
                {
                    exceptionCallback(e);
                }
                else
                {
                    LogUtil.LogError(e);
                }
            }
        }
    }
}
