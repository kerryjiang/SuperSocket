using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    public delegate void ExceptionCallback();

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
            public string StepName { get; set; }
            public WaitCallback RunCallback { get; set; }
            public ExceptionCallback ExceptionCallback { get; set; }
        }

        public static void ExecuteAsync(this IAsyncRunner runner, AsyncRunType runType, string stepName, WaitCallback callback)
        {
            ExecuteAsync(runner, runType, stepName, callback, null);
        }

        public static void ExecuteAsync(this IAsyncRunner runner, AsyncRunType runType, string stepName, WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            if (runType == AsyncRunType.ThreadPool)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ExecuteAsyncInternal(stepName, callback, exceptionCallback);
                });
            }
            else
            {
                Thread newThread = new Thread(new ParameterizedThreadStart(ExecuteAsyncStart));
                newThread.Start(new ExecuteAsyncThreadStartParameter
                    {
                        StepName = stepName,
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

            ExecuteAsyncInternal(startParameter.StepName, startParameter.RunCallback, startParameter.ExceptionCallback);
        }

        private static void ExecuteAsyncInternal(string stepName, WaitCallback callback, ExceptionCallback exceptionCallback)
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
        }
    }
}
