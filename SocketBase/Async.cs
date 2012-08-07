using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Async extension class
    /// </summary>
    public static class Async
    {
        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action task)
        {
            return AsyncRun(logProvider, task, TaskCreationOptions.None);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="taskOption">The task option.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action task, TaskCreationOptions taskOption)
        {
            return AsyncRun(logProvider, task, taskOption, null);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action task, Action<Exception> exceptionHandler)
        {
            return AsyncRun(logProvider, task, TaskCreationOptions.None, exceptionHandler);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="taskOption">The task option.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action task, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, taskOption).ContinueWith(t =>
                {
                    if (exceptionHandler != null)
                        exceptionHandler(t.Exception);
                    else
                    {
                        if (logProvider.Logger.IsErrorEnabled)
                        {
                            for (var i = 0; i < t.Exception.InnerExceptions.Count; i++)
                            {
                                logProvider.Logger.Error(t.Exception.InnerExceptions[i]);
                            }
                        }
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action<object> task, object state)
        {
            return AsyncRun(logProvider, task, state, TaskCreationOptions.None);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="taskOption">The task option.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action<object> task, object state, TaskCreationOptions taskOption)
        {
            return AsyncRun(logProvider, task, state, taskOption, null);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action<object> task, object state, Action<Exception> exceptionHandler)
        {
            return AsyncRun(logProvider, task, state, TaskCreationOptions.None, exceptionHandler);
        }

        /// <summary>
        /// Runs the specified task.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="task">The task.</param>
        /// <param name="state">The state.</param>
        /// <param name="taskOption">The task option.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <returns></returns>
        public static Task AsyncRun(this ILoggerProvider logProvider, Action<object> task, object state, TaskCreationOptions taskOption, Action<Exception> exceptionHandler)
        {
            return Task.Factory.StartNew(task, state, taskOption).ContinueWith(t =>
            {
                if (exceptionHandler != null)
                    exceptionHandler(t.Exception);
                else
                {
                    if (logProvider.Logger.IsErrorEnabled)
                    {
                        for (var i = 0; i < t.Exception.InnerExceptions.Count; i++)
                        {
                            logProvider.Logger.Error(t.Exception.InnerExceptions[i]);
                        }
                    }
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
