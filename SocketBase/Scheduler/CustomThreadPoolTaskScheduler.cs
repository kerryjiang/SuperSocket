using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketBase.Scheduler
{
    class CustomThreadPoolTaskScheduler : TaskScheduler, IDisposable
    {
        class TaskQueueEnumerable : IEnumerable<Task>
        {
            private ConcurrentQueue<Task>[] m_TaskQueues;

            public TaskQueueEnumerable(ConcurrentQueue<Task>[] queues)
            {
                m_TaskQueues = queues;
            }

            public IEnumerator<Task> GetEnumerator()
            {
                for (var i = 0; i < m_TaskQueues.Length; i++)
                {
                    var queue = m_TaskQueues[i];

                    var tasks = queue.ToArray();

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

        private int m_WorkingThreadCount;

        private int m_MinThreadCount;

        private int m_MaxThreadCount;

        private int m_MaxSleepingTimeOut = 500;

        private int m_MinSleepingTimeOut = 10;

        private ConcurrentQueue<Task>[] m_TaskQueues;

        private Thread[] m_WorkingThreads;

        private int m_Stopped = 0;

        private const int c_MagicThreadId = 100000000;

        private ILog m_Log;

        public CustomThreadPoolTaskScheduler(int workingThreadCount)
        {
            m_Log = AppContext.CurrentServer.Logger;

            m_WorkingThreadCount = workingThreadCount;
            m_TaskQueues = new ConcurrentQueue<Task>[workingThreadCount];
            m_WorkingThreads = new Thread[workingThreadCount];

            for (var i = 0; i < workingThreadCount; i++)
            {
                var queue = new ConcurrentQueue<Task>();
                m_TaskQueues[i] = queue;
                var thread = new Thread(RunWorkingThreads);
                thread.IsBackground = true;
                thread.Start(queue);
                m_WorkingThreads[i] = thread;
            }
        }

        private void RunWorkingThreads(object state)
        {
            if (m_Log.IsDebugEnabled)
                m_Log.DebugFormat("The request handling thread {0} was started.", Thread.CurrentThread.ManagedThreadId);

            var queue = state as ConcurrentQueue<Task>;

            var sleepingTimeOut = m_MinSleepingTimeOut;

            while (m_Stopped == 0)
            {
                Task task;

                if (queue.TryDequeue(out task))
                {
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

                if (sleepingTimeOut < m_MaxSleepingTimeOut)
                {
                    sleepingTimeOut = Math.Min(m_MaxSleepingTimeOut, sleepingTimeOut * 2);
                }
            }

            if (m_Log.IsDebugEnabled)
                m_Log.DebugFormat("The request handling thread {0} was stopped.", Thread.CurrentThread.ManagedThreadId);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return new TaskQueueEnumerable(m_TaskQueues);
        }

        private ConcurrentQueue<Task> FindQueueByThreadId(int threadId)
        {
            for (var i = 0; i < m_WorkingThreads.Length; i++)
            {
                var thread = m_WorkingThreads[i];

                if (thread.ManagedThreadId == threadId)
                    return m_TaskQueues[i];
            }

            return null;
        }

        private int FindFreeQueue()
        {
            ConcurrentQueue<Task> freeQueue = m_TaskQueues[0];

            var freeQueueCount = freeQueue.Count;
            var freeQueueIndex = 0;

            if (freeQueue.Count == 0)
                return freeQueueIndex;

            for (var i = 1; i < m_TaskQueues.Length; i++)
            {
                var queue = m_TaskQueues[i];
                var queueCount = queue.Count;

                if (queueCount == 0)
                    return i;

                if (queueCount < freeQueueCount)
                {
                    freeQueueCount = queueCount;
                    freeQueueIndex = i;
                }
            }

            return freeQueueIndex;
        }

        protected override void QueueTask(Task task)
        {
            var context = task.AsyncState as IThreadExecutingContext;
            var preferThreadId = context.PreferedThreadId;

            if (preferThreadId > 0)
            {
                if (preferThreadId > c_MagicThreadId)
                    preferThreadId = preferThreadId % c_MagicThreadId;

                var preferedQueue = FindQueueByThreadId(preferThreadId);

                if (preferedQueue != null)
                {
                    context.Increment(c_MagicThreadId);
                    preferedQueue.Enqueue(task);
                    return;
                }
            }

            var freeQueueIndex = FindFreeQueue();
            var threadId = m_WorkingThreads[freeQueueIndex].ManagedThreadId;
            context.Increment(threadId);
            m_TaskQueues[freeQueueIndex].Enqueue(task);
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

                for (var i = 0; i < m_TaskQueues.Length; i++)
                {
                    sum += m_TaskQueues[i].Count;
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
