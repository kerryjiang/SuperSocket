using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.Common
{
    public static class Async
    {
        public static void Run(Action task)
        {
            Run(task, TaskCreationOptions.None);
        }

        public static void Run(Action task, TaskCreationOptions taskOption)
        {
            Run(task, taskOption, null);
        }

        public static void Run(Action task, Action<Exception> exceptionHandler)
        {
            Run(task, TaskCreationOptions.None, exceptionHandler);
        }

        public static void Run(Action task, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            Task.Factory.StartNew(task, taskOption).ContinueWith(t =>
                {
                    if (exceptionHandler != null)
                        exceptionHandler(t.Exception);
                    else
                        LogUtil.LogError(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
