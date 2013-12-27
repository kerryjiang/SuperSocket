using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SuperSocket.SocketBase
{
    class ThreadingTaskScheduler : TaskScheduler, IDisposable
    {
        private ConcurrentQueue<Task> m_TaskQueue;

        private int m_WorkingThreadCount;

        private int m_MaxSleepingTimeOut = 500;

        private int m_MinSleepingTimeOut = 10;

        private Thread[] m_WorkingThreads;

        private int m_Stopped = 0;

        public ThreadingTaskScheduler(int workingThreadCount)
        {
            m_TaskQueue = new ConcurrentQueue<Task>();
            m_WorkingThreadCount = workingThreadCount;
            m_WorkingThreads = new Thread[workingThreadCount];

            for (var i = 0; i < workingThreadCount; i++)
            {
                var thread = new Thread(RunWorkingThreads);
                thread.IsBackground = true;
                thread.Start();
                m_WorkingThreads[i] = thread;
            }
        }

        private void RunWorkingThreads()
        {
            var sleepingTimeOut = m_MinSleepingTimeOut;

            while (m_Stopped == 0)
            {
                Task task;

                if (m_TaskQueue.TryDequeue(out task))
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
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return m_TaskQueue.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            m_TaskQueue.Enqueue(task);
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
                return m_TaskQueue.Count;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref m_Stopped, 1, 0) == 0)
            {
                Thread.Sleep(m_MaxSleepingTimeOut);
                GC.SuppressFinalize(this);
            }
        }
    }
}
