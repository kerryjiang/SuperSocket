using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketBase.Utils
{
    /// <summary>
    /// SendingQueue Source Creator
    /// </summary>
    public class SendingQueueSourceCreator : IPoolItemCreator<SendingQueue>
    {
        private int m_SendingQueueSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingQueueSourceCreator" /> class.
        /// </summary>
        /// <param name="sendingQueueSize">Size of the sending queue.</param>
        public SendingQueueSourceCreator(int sendingQueueSize)
        {
            m_SendingQueueSize = sendingQueueSize;
        }

        /// <summary>
        /// Creates the specified quantity of sendingQueues.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public IEnumerable<SendingQueue> Create(int count)
        {
            return new SendingQueueItemEnumerable(m_SendingQueueSize, count);
        }

        class SendingQueueItemEnumerable : IEnumerable<SendingQueue>
        {
            private int m_SendingQueueSize;

            private int m_Count;

            public SendingQueueItemEnumerable(int sendingQueueSize, int count)
            {
                m_SendingQueueSize = sendingQueueSize;
                m_Count = count;
            }

            public IEnumerator<SendingQueue> GetEnumerator()
            {
                int count = m_Count;

                var source = new ArraySegment<byte>[count * m_SendingQueueSize];

                for (int i = 0; i < count; i++)
                {
                    yield return new SendingQueue(source, i * m_SendingQueueSize, m_SendingQueueSize);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
