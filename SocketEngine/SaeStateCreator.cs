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
        IAsyncSocketEventComplete m_SocketEventComplete;

        public SaeStateCreator(IBufferManager bufferManager, int bufferSize, IAsyncSocketEventComplete socketEventComplete)
        {
            m_BufferManager = bufferManager;
            m_BufferSize = bufferSize;
            m_SocketEventComplete = socketEventComplete;
        }

        public IEnumerable<SaeState> Create(int count)
        {
            return new SaeItemEnumerable(m_BufferManager, m_BufferSize, count, m_SocketEventComplete);
        }

        class SaeItemEnumerable : IEnumerable<SaeState>
        {
            IBufferManager m_BufferManager;

            private int m_BufferSize;

            private int m_Count;

            IAsyncSocketEventComplete m_SocketEventComplete;

            public SaeItemEnumerable(IBufferManager bufferManager, int bufferSize, int count, IAsyncSocketEventComplete socketEventComplete)
            {
                m_BufferManager = bufferManager;
                m_BufferSize = bufferSize;
                m_Count = count;
                m_SocketEventComplete = socketEventComplete;
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
                    sae.UserToken = saeState;
                    sae.Completed += new EventHandler<SocketAsyncEventArgs>(m_SocketEventComplete.HandleSocketEventComplete);
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
