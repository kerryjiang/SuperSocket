using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public class SynchronizedPool<T> where T : class
    {
        private Stack<T> m_Pool;

        public SynchronizedPool(int capacity)
        {
            m_Pool = new Stack<T>(capacity);
        }

        public void Push(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            lock (m_Pool)
            {
                m_Pool.Push(item);
            }
        }

        public T Pop()
        {
            lock (m_Pool)
            {
                return m_Pool.Pop();
            }
        }

        public int Count
        {
            get
            {
                return m_Pool.Count;
            }
        }
    }
}
