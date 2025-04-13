using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket
{
    /// <summary>
    /// Provides utility methods for executing asynchronous operations in parallel.
    /// </summary>
    public static class AsyncParallel
    {
        /// <summary>
        /// Executes the specified asynchronous operation for each item in the source collection in parallel.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the source collection.</typeparam>
        /// <param name="source">The source collection of items.</param>
        /// <param name="operation">The asynchronous operation to execute for each item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum number of operations to execute in parallel. Default is 5.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task ForEach<TItem>(IEnumerable<TItem> source, Func<TItem, Task> operation, int maxDegreeOfParallelism = 5)
        {
            await ForEach(source, operation, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = CancellationToken.None
            });
        }

        /// <summary>
        /// Executes the specified asynchronous operation for each item in the source collection in parallel with the specified parallel options.
        /// </summary>
        /// <typeparam name="TItem">The type of the items in the source collection.</typeparam>
        /// <param name="source">The source collection of items.</param>
        /// <param name="operation">The asynchronous operation to execute for each item.</param>
        /// <param name="parallelOptions">The options for controlling the parallel execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task ForEach<TItem>(IEnumerable<TItem> source, Func<TItem, Task> operation, ParallelOptions parallelOptions)
        {
            var allTasks = new List<Task>();
            var throttler = new SemaphoreSlim(initialCount: parallelOptions.MaxDegreeOfParallelism);

            foreach (var item in source)
            {
                await throttler.WaitAsync(parallelOptions.CancellationToken);

                if (parallelOptions.CancellationToken.IsCancellationRequested)
                    break;

                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await operation(item);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, parallelOptions.CancellationToken));
            }

            await Task.WhenAll(allTasks);
        }
    }
}