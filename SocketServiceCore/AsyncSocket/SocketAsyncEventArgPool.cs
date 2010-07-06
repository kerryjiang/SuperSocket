using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Represents a collection of resusable SocketAsyncEventArgs objects.  
    /// </summary>
    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> m_pool;
        
        /// <summary>
        /// Initializes the object pool to the specified size
        /// </summary>
        /// <param name="capacity">The maximum number of SocketAsyncEventArgs objects the pool can hold</param>
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool
        /// </summary>
        /// <param name="item">The SocketAsyncEventArgs instance to add to the pool</param>
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool
        /// </summary>
        /// <returns>The object removed from the pool</returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        /// <summary>
        /// The number of SocketAsyncEventArgs instances in the pool
        /// </summary>
        public int Count
        {
            get { return m_pool.Count; }
        }

    }
}
