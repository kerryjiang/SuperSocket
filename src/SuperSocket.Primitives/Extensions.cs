using System;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket
{
    /// <summary>
    /// Provides utility extension methods for handling tasks.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Ignores the result of the specified task.
        /// </summary>
        /// <param name="task">The task to ignore.</param>
        public static void DoNotAwait(this Task task)
        {
        }

        /// <summary>
        /// Ignores the result of the specified value task.
        /// </summary>
        /// <param name="task">The value task to ignore.</param>
        public static void DoNotAwait(this ValueTask task)
        {
        }

#if NETSTANDARD2_1
        private static readonly ValueTask _completedTask = new ValueTask();

        /// <summary>
        /// Gets a completed <see cref="ValueTask"/> instance.
        /// </summary>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public static ValueTask GetCompletedTask()
        {
            return _completedTask;
        }
#endif
    }
}