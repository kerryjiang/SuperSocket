using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Scheduler
{
    class CustomThreadPoolTaskScheduler : TaskScheduler, IDisposable
    {
        class TaskQueueEnumerable : IEnumerable<Task>
        {
            private QueueItem[] m_Items;

            public TaskQueueEnumerable(QueueItem[] items)
            {
                m_Items = items;
            }

            public IEnumerator<Task> GetEnumerator()
            {
                for (var i = 0; i < m_Items.Length; i++)
                {
                    var item = m_Items[i];

                    var tasks = item.Queue.ToArray();

                    for (var j = 0; j < tasks.Length; j++)
                    {
                        yield return tasks[j];
                    }
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class QueueItem
        {
            public Thread Thread { get; private set; }

            public int ThreadId { get; private set; }

            public bool Stopped { get; set; }

            public ConcurrentQueue<Task> Queue { get; private set; }

            public QueueItem(Thread thread)
            {
                Thread = thread;
                ThreadId = thread.ManagedThreadId;
                Queue = new ConcurrentQueue<Task>();
            }
        }

        private int m_WorkingThreadCount;

        private int m_MinRequestHandlingThreads;

        private int m_MaxRequestHandlingThreads;

        private int m_MaxSleepingTimeOut = 500;

        private int m_ThreadRecycleTimeOut = 1000 * 10; // 10 seconds

        private int m_MinSleepingTimeOut = 10;

        private int m_ThreadChanging = 0;

        private QueueItem[] m_WorkingItems;

        private int m_Stopped = 0;

        private const int c_MagicThreadId = 100000000;

        private ILog m_Log;

        public CustomThreadPoolTaskScheduler(IServerConfig config)
        {
            if (config.MinRequestHandlingThreads < 1 || config.MinRequestHandlingThreads > 100)
                throw new Exception("MinRequestHandlingThreads must be between 1 and 100!");

            if (config.MaxRequestHandlingThreads < 1 || config.MaxRequestHandlingThreads > 100)
                throw new Exception("MaxRequestHandlingThreads must be between 1 and 100!");

            if(config.MaxRequestHandlingThreads < config.MinRequestHandlingThreads)
                throw new Exception("MaxRequestHandlingThreads must be greater than MinRequestHandlingThreads!");

            m_Log = AppContext.CurrentServer.Logger;

            m_MinRequestHandlingThreads = config.MinRequestHandlingThreads;
            m_MaxRequestHandlingThreads = config.MaxRequestHandlingThreads;
            m_WorkingThreadCount = config.MinRequestHandlingThreads;
            m_WorkingItems = new QueueItem[m_MaxRequestHandlingThreads];

            for (var i = 0; i < m_WorkingThreadCount; i++)
            {
                var thread = new Thread(RunWorkingThreads);
                var item = new QueueItem(thread);
                m_WorkingItems[i] = item;
                thread.IsBackground = true;
                thread.Start(item);
            }
        }

        private void RunWorkingThreads(object state)
        {
            if (m_Log.IsDebugEnabled)
                m_Log.DebugFormat("The request handling thread {0} was started.", Thread.CurrentThread.ManagedThreadId);

            var queueItem = state as QueueItem;
            var queue = queueItem.Queue;

            var sleepingTimeOut = m_MinSleepingTimeOut;

            var totalSleep = 0;

            while (!queueItem.Stopped)
            {
                Task task;

                if (queue.TryDequeue(out task))
                {
                    totalSleep = 0;

                    if (sleepingTimeOut != m_MinSleepingTimeOut)
                        sleepingTimeOut = m_MinSleepingTimeOut;

                    try
                    {
                        TryExecuteTask(task);
                    }
                    finally
                    {
                        var executingContext = task.AsyncState as IThreadExecutingContext;
                        if (executingContext != null)
                            executingContext.Decrement(c_MagicThreadId);
                    }

                    continue;
                }

                Thread.Sleep(sleepingTimeOut);
                totalSleep += sleepingTimeOut;

                //The thread can be recycled
                if (totalSleep >= m_ThreadRecycleTimeOut)
                {
                    var currentThreadCount = m_WorkingThreadCount;

                    if (currentThreadCount > m_MinRequestHandlingThreads)
                    {
                        // try to recycle
                        TryRecycleThread(m_WorkingItems, queueItem, currentThreadCount);
                    }
                }

                if (sleepingTimeOut < m_MaxSleepingTimeOut)
                {
                    sleepingTimeOut = Math.Min(m_MaxSleepingTimeOut, sleepingTimeOut * 2);
                }
            }

            if (m_Log.IsDebugEnabled)
                m_Log.DebugFormat("The request handling thread {0} was stopped.", Thread.CurrentThread.ManagedThreadId);
        }

        private bool SetThreadChangingState()
        {
            return Interlocked.CompareExchange(ref m_ThreadChanging, 0, 1) == 0;
        }

        private bool TryRecycleThread(QueueItem[] items, QueueItem recycleItem, int currentThreadCount)
        {
            if(!SetThreadChangingState())
                return false;

            // already changed
            if (m_WorkingItems != items)
                return false;

            if (recycleItem.Queue.Count > 0)
            {
                m_ThreadChanging = 0;
                return false;
            }

            recycleItem.Stopped = true;

            Thread.SpinWait(1);

            if (recycleItem.Queue.Count > 0)
            {
                recycleItem.Stopped = false;
                m_ThreadChanging = 0;
                return false;
            }

            var newItems = new QueueItem[items.Length];

            var foundRecycleItem = false;

            for(var i = 0; i < currentThreadCount; i++)
            {
                var queue = items[i];

                if (foundRecycleItem)
                {
                    newItems[i - 1] = queue;
                    continue;
                }

                if (queue == recycleItem)
                {
                    foundRecycleItem = true;
                    continue;
                }

                newItems[i] = queue;
            }

            m_WorkingItems = newItems;
            m_WorkingThreadCount = currentThreadCount - 1;

            m_ThreadChanging = 0;

            return true;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return new TaskQueueEnumerable(m_WorkingItems);
        }

        private QueueItem FindItemByThreadId(int threadId)
        {
            var queueItems = m_WorkingItems;

            for (var i = 0; i < m_WorkingThreadCount; i++)
            {
                var item = queueItems[i];

                if (item.ThreadId == threadId && !item.Stopped)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Finds the free queue, the method can be improved by better load balance algorithm
        /// </summary>
        /// <returns></returns>
        private QueueItem FindFreeItem()
        {
            var queues = m_WorkingItems;

            QueueItem freeItem = queues[0];

            var freeQueueCount = freeItem.Queue.Count;

            if (freeQueueCount == 0)
                return freeItem;

            for (var i = 1; i < m_WorkingThreadCount; i++)
            {
                var item = queues[i];

                if (item.Stopped)
                    continue;

                var queueCount = item.Queue.Count;

                if (queueCount == 0)
                    return item;

                if (queueCount < freeQueueCount)
                {
                    freeQueueCount = queueCount;
                    freeItem = item;
                }
            }

            var currentThreadCount = m_WorkingThreadCount;

            // hasn't reach the max request handling threads
            if (currentThreadCount < m_MaxRequestHandlingThreads)
            {
                // in thread count changing state, don't increase for now
                if (!SetThreadChangingState())
                    return freeItem;

                if (currentThreadCount != m_WorkingThreadCount)
                    return freeItem;

                var thread = new Thread(RunWorkingThreads);
                thread.IsBackground = true;
                var item = new QueueItem(thread);
                m_WorkingItems[currentThreadCount] = item;
                m_WorkingThreadCount = currentThreadCount + 1;
                thread.Start(item);

                m_ThreadChanging = 0;
                return item;
            }

            return freeItem;
        }

        /// <summary>
        /// Queues a <see cref="T:System.Threading.Tasks.Task" /> to the scheduler.
        /// </summary>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task" /> to be queued.</param>
        protected override void QueueTask(Task task)
        {
            var context = task.AsyncState as IThreadExecutingContext;
            var origPreferThreadId = context.PreferedThreadId;
            var preferThreadId = origPreferThreadId;

            if (preferThreadId > 0)
            {
                if (preferThreadId > c_MagicThreadId)
                    preferThreadId = preferThreadId % c_MagicThreadId;

                var preferedItem = FindItemByThreadId(preferThreadId);

                if (preferedItem != null)
                {
                    context.Increment(c_MagicThreadId);
                    preferedItem.Queue.Enqueue(task);
                    return;
                }

                // cannot find the prefered queue go through here
            }

            var freeItem = FindFreeItem();
            var threadId = freeItem.ThreadId;

            // the prefered thread cannot be found, so clear the threadId from the context
            if (origPreferThreadId > 0)
                context.Decrement(origPreferThreadId);

            // record the threadId in the context
            context.Increment(threadId);

            freeItem.Queue.Enqueue(task);
            return;
        }

        protected override bool TryDequeue(Task task)
        {
            return false;
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return m_WorkingThreadCount;
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        internal int RestTasks
        {
            get
            {
                var sum = 0;

                for (var i = 0; i < m_WorkingItems.Length; i++)
                {
                    sum += m_WorkingItems[i].Queue.Count;
                }

                return sum;
            }
        }

        ~CustomThreadPoolTaskScheduler()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref m_Stopped, 1, 0) == 0)
            {
                var items = m_WorkingItems;

                for (var i = 0; i < m_WorkingThreadCount; i++)
                {
                    items[i].Stopped = true;
                }

                Thread.Sleep(m_MaxSleepingTimeOut * 2);
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
