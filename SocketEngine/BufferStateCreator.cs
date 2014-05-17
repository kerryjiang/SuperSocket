using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketEngine
{
    class BufferStateCreator : IPoolItemCreator<BufferState>
    {
        IPool<BufferState> m_Pool;
        IBufferManager m_BufferManager;
        int m_BufferSize;

        public BufferStateCreator(IPool<BufferState> pool, IBufferManager bufferManager, int bufferSize)
        {
            m_Pool = pool;
            m_BufferManager = bufferManager;
            m_BufferSize = bufferSize;
        }

        public IEnumerable<BufferState> Create(int count)
        {
            return new BufferStateItemEnumerable(m_Pool, m_BufferManager, m_BufferSize, count);
        }

        class BufferStateItemEnumerable : IEnumerable<BufferState>
        {
            IPool<BufferState> m_Pool;

            IBufferManager m_BufferManager;

            private int m_BufferSize;

            private int m_Count;

            public BufferStateItemEnumerable(IPool<BufferState> pool, IBufferManager bufferManager, int bufferSize, int count)
            {
                m_Pool = pool;
                m_BufferManager = bufferManager;
                m_BufferSize = bufferSize;
                m_Count = count;
            }

            public IEnumerator<BufferState> GetEnumerator()
            {
                int count = m_Count;

                for (int i = 0; i < count; i++)
                {
                    yield return new BufferState(m_BufferManager.GetBuffer(m_BufferSize));
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
