using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketBase.Scheduler
{
    class SingleThreadTaskScheduler : TaskScheduler, IDisposable
    {
        private ConcurrentQueue<Task> m_TaskQueue;

        private int m_MaxSleepingTimeOut = 500;

        private int m_MinSleepingTimeOut = 10;

        private Thread m_WorkingThread;

        private int m_Stopped = 0;

        private ILog m_Log;

        public SingleThreadTaskScheduler()
        {
            m_Log = AppContext.CurrentServer.Logger;
            m_TaskQueue = new ConcurrentQueue<Task>();

            var thread = new Thread(RunWorkingThreads);
            thread.IsBackground = true;
            thread.Start();
            m_WorkingThread = thread;
        }

        private void RunWorkingThreads()
        {
            if (m_Log.IsDebugEnabled)
                m_Log.DebugFormat("The request handling thread {0} was started.", Thread.CurrentThread.ManagedThreadId);

            var queue = m_TaskQueue;

            var sleepingTimeOut = m_MinSleepingTimeOut;

            while (m_Stopped == 0)
            {
                Task task;

                if (queue.TryDequeue(out task))
                {
                    if (sleepingTimeOut != m_MinSleepingTimeOut)
                        sleepingTimeOut = m_MinSleepingTimeOut;

                    TryExecuteTask(task);
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
            return m_TaskQueue.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            m_TaskQueue.Enqueue(task);
        }

        private const int c_MaximumConcurrencyLevel = 1;

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return c_MaximumConcurrencyLevel;
            }
        }

        protected override bool TryDequeue(Task task)
        {
            return false;
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref m_Stopped, 1, 0) == 0)
            {
                Thread.Sleep(m_MaxSleepingTimeOut);
                GC.SuppressFinalize(this);
            }
        }

        ~SingleThreadTaskScheduler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
