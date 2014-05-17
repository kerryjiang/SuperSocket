using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketEngine
{
    class SaeStateCreator : IPoolItemCreator<SaeState>
    {
        IBufferManager m_BufferManager;
        int m_BufferSize;

        public SaeStateCreator(IBufferManager bufferManager, int bufferSize)
        {
            m_BufferManager = bufferManager;
            m_BufferSize = bufferSize;
        }

        public IEnumerable<SaeState> Create(int count)
        {
            return new SaeItemEnumerable(m_BufferManager, m_BufferSize, count);
        }

        class SaeItemEnumerable : IEnumerable<SaeState>
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

            public IEnumerator<SaeState> GetEnumerator()
            {
                int count = m_Count;

                for (int i = 0; i < count; i++)
                {
                    var buffer = m_BufferManager.GetBuffer(m_BufferSize);
                    var sae = new SocketAsyncEventArgs();
                    sae.SetBuffer(buffer, 0, m_BufferSize);
                    var saeState = new SaeState(sae);
                    yield return saeState;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
