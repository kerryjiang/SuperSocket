using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.Common
{
    public static class Async
    {
        public static Task Run(Action task)
        {
            return Run(task, TaskCreationOptions.None);
        }

        public static Task Run(Action task, TaskCreationOptions taskOption)
        {
            return Run(task, taskOption, null);
        }

        public static Task Run(Action task, Action<Exception> exceptionHandler)
        {
            return Run(task, TaskCreationOptions.None, exceptionHandler);
        }

        public static Task Run(Action task, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, taskOption).ContinueWith(t =>
                {
                    if (exceptionHandler != null)
                        exceptionHandler(t.Exception);
                    else
                        LogUtil.LogError(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
