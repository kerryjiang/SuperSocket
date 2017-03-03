using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.Common
{
    /// <summary>
    /// This class creates a single large buffer which can be divided up and assigned to SocketAsyncEventArgs objects for use
    /// with each socket I/O operation.  This enables bufffers to be easily reused and gaurds against fragmenting heap memory.
    /// 
    /// The operations exposed on the BufferManager class are not thread safe.
    /// </summary>
    public class BufferManager
    {
        int m_numBytes;                 // the total number of bytes controlled by the buffer pool
        byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager
        Stack<int> m_freeIndexPool;     // 
        int m_currentIndex;
        int m_bufferSize;

        /// <summary>
        /// Get the buffer
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return m_buffer;
            }
        }

        /// <summary>
        /// Get the buffer
        /// </summary>
        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferManager"/> class.
        /// </summary>
        /// <param name="totalBytes">The total bytes.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer pool
        /// </summary>
        public void InitBuffer()
        {
            // create one big large buffer and divide that out to each SocketAsyncEventArg object
            m_buffer = new byte[m_numBytes];
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object
        /// </summary>
        /// <returns>A Tuple where Item1 is true if the buffer should be set, else false.
        /// If Item1 is true then Item2 has the new offset, else 0.
        /// </returns>
        public Tuple<bool, int> SetBuffer()
        {
            int offset;
            if (m_freeIndexPool.Count > 0)
            {
                offset = m_freeIndexPool.Pop();
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return new Tuple<bool, int>(false, 0);
                }
                offset = m_currentIndex;
                m_currentIndex += m_bufferSize;
            }
            return new Tuple<bool, int>(true, offset);
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the
        /// buffer pool
        /// </summary>
        public void FreeBuffer(int offset)
        {
            m_freeIndexPool.Push(offset);
        }
    }
}
