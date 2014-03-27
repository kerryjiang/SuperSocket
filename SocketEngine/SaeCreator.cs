using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketEngine
{
    class SaeCreator : IPoolItemCreator<SocketAsyncEventArgs>
    {
        IBufferManager m_BufferManager;
        int m_BufferSize;

        public SaeCreator(IBufferManager bufferManager, int bufferSize)
        {
            m_BufferManager = bufferManager;
            m_BufferSize = bufferSize;
        }

        public IEnumerable<SocketAsyncEventArgs> Create(int count)
        {
            return new SaeItemEnumerable(m_BufferManager, m_BufferSize, count);
        }

        class SaeItemEnumerable : IEnumerable<SocketAsyncEventArgs>
        {
            IBufferManager m_BufferManager;

            private int m_BufferSize;

            private int m_Count;

            public SaeItemEnumerable(IBufferManager bufferManager, int bufferSize, int count)
            {
                m_BufferManager = bufferManager;
                m_BufferSize = bufferSize;
                m_Count = count;
            }

            public IEnumerator<SocketAsyncEventArgs> GetEnumerator()
            {
                int count = m_Count;

                for (int i = 0; i < count; i++)
                {
                    var buffer = m_BufferManager.GetBuffer(m_BufferSize);
                    var sae = new SocketAsyncEventArgs();
                    sae.SetBuffer(0, m_BufferSize);
                    yield return sae;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
