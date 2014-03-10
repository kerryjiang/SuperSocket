using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    class BufferItemCreator : IPoolItemCreator<byte[]>
    {
        private int m_BufferSize;

        public BufferItemCreator(int bufferSize)
        {
            m_BufferSize = bufferSize;
        }

        public IEnumerable<byte[]> Create(int count)
        {
            return new BufferItemEnumerable(m_BufferSize, count);
        }
    }

    class BufferItemEnumerable : IEnumerable<byte[]>
    {
        private int m_BufferSize;

        private int m_Count;

        public BufferItemEnumerable(int bufferSize, int count)
        {
            m_BufferSize = bufferSize;
            m_Count = count;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            int count = m_Count;

            for (int i = 0; i < count; i++)
            {
                yield return new byte[m_BufferSize];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
