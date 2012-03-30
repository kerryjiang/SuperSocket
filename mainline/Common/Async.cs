using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common.Logging;

namespace SuperSocket.Common
{
    /// <summary>
    /// Async extension class
    /// </summary>
    public static class Async
    {
        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public static Task Run(Action task)
        {
            return Run(task, TaskCreationOptions.None);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="taskOption">The task option.</param>
        /// <returns></returns>
        public static Task Run(Action task, TaskCreationOptions taskOption)
        {
            return Run(task, taskOption, null);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task Run(Action task, Action<Exception> exceptionHandler)
        {
            return Run(task, TaskCreationOptions.None, exceptionHandler);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="taskOption">The task option.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task Run(Action task, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, taskOption).ContinueWith(t =>
                {
                    if (exceptionHandler != null)
                        exceptionHandler(t.Exception.InnerException);
                    else
                        LogFactoryProvider.GlobalLog.Error(t.Exception.InnerException);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public static Task Run(Action<object> task, object state)
        {
            return Run(task, state, TaskCreationOptions.None);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="taskOption">The task option.</param>
        /// <returns></returns>
        public static Task Run(Action<object> task, object state, TaskCreationOptions taskOption)
        {
            return Run(task, state, taskOption, null);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task Run(Action<object> task, object state, Action<Exception> exceptionHandler)
        {
            return Run(task, state, TaskCreationOptions.None, exceptionHandler);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="taskOption">The task option.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task Run(Action<object> task, object state, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, state, taskOption).ContinueWith(t =>
            {
                if (exceptionHandler != null)
                    exceptionHandler(t.Exception.InnerException);
                else
                    LogFactoryProvider.GlobalLog.Error(t.Exception.InnerException);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
