using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Concurrent
{
    public class ConcurrentStack<T> where T : class
    {
        private Stack<T> m_Pool;

        public ConcurrentStack(IEnumerable<T> source)
        {
            m_Pool = new Stack<T>(source.Count());

            foreach (var item in source)
            {
                m_Pool.Push(item);
            }
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

        public bool TryPop(out T topItem)
        {
            lock (m_Pool)
            {
                if(m_Pool.Count <= 0)
                {
                    topItem = default(T);
                    return false;
                }

                topItem = m_Pool.Pop();
                return true;
            }
        }
    }
}
