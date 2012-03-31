using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine.AsyncSocket
{
    /// <summary>
    /// Socket Buffer
    /// </summary>
    public class SocketBuffer
    {
        private object m_Locker = new object();
        private Int32 m_PageSize = 1024;
        private Int32 m_ReadCursor = 0;
        private Int32 m_WriteCursor = 0;
        private Int32 m_Length = 0;
        private List<byte[]> mPages = new List<byte[]>();


        /// <summary>Returns the length of valid content.</summary>
        public int Length
        {// No need to lock this field. It's updated when locked whenever this SocketBuffer changes.
            get { return m_Length; }
        }

        /// <summary>Creates new SocketBuffer object.</summary>
        public SocketBuffer()
        { }

        /// <summary>Creates new SocketBuffer object, with default page size
        /// if you have any expectational as to the chunks length
        /// you can use this constructor to fine tune performance.</summary>
        public SocketBuffer(int pageSize)
        {
            try
            {
                if (pageSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("Page size must be greater then 0");
                }
                m_PageSize = pageSize;
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>Write given data to the buffer.</summary>
        /// <param name="data">the data to write</param>
        /// <param name="start">start of valid content</param>
        /// <param name="length">length of valid content</param>
        public void Write(byte[] data, int start, int length)
        {
            try
            {
                lock (m_Locker)
                {
                    if (data == null || length == 0)
                    {
                        return;
                    }
                    int leftToWrite = length;
                    while (leftToWrite != 0)
                    {
                        int currentPageIndex = m_WriteCursor / m_PageSize;
                        if (currentPageIndex >= mPages.Count)
                        {
                            AddPage();
                        }
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = m_PageSize - (m_WriteCursor % m_PageSize);
                        int toWrite = Math.Min(leftToWrite, spaceInPage);
                        Array.Copy(data, start, currentPage, (m_WriteCursor % m_PageSize), toWrite);
                        leftToWrite -= toWrite;
                        m_WriteCursor += toWrite;
                        start += toWrite;
                        m_Length = m_WriteCursor - m_ReadCursor;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>Write the entire data to the buffer.</summary>
        public void Write(byte[] data)
        {
            try
            {
                Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>Read data from the buffer, this method
        /// removes the read data from the buffer.</summary>
        public byte[] Read(int size)
        {
            try
            {
                lock (m_Locker)
                {
                    byte[] result = Peek(size);
                    Discard(size);
                    return result;
                }
            }
            catch (Exception)
            {
                
                return null;
            }
        }

        /// <summary>Read data from the buffer, this method
        /// removes the read data from the buffer.</summary>
        public byte[] Read(int startIndex, int size)
        {
            try
            {
                lock (m_Locker)
                {
                    byte[] result = Peek(startIndex, size);
                    Discard(startIndex, size);
                    return result;
                }
            }
            catch (Exception)
            {
                
                return null;
            }
        }

        /// <summary>Given a size this method Discards (forgets) data from the buffer.</summary>
        public void Discard(int size)
        {
            try
            {
                lock (m_Locker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    m_ReadCursor += size;
                    m_Length = m_WriteCursor - m_ReadCursor;
                    Purge();
                }
            }
            catch (Exception)
            {
                //if (size > Length)
                //    throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                
            }
        }

        /// <summary>Given a size this method Discards (forgets) data from the buffer.</summary>
        public void Discard(int startIndex, int size)
        {
            try
            {
                lock (m_Locker)
                {
                    if ((startIndex + size) > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    m_ReadCursor += startIndex + size;
                    m_Length = m_WriteCursor - m_ReadCursor;
                    Purge();
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>Peeking is like reading but without discarding, 
        /// if you will call peek or read again, you will get the same data.</summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] Peek(int size)
        {
            try
            {
                lock (m_Locker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    byte[] result = new byte[size];
                    int readCursor = m_ReadCursor;
                    int leftToRead = size;
                    while (leftToRead != 0)
                    {
                        int currentPageIndex = readCursor / m_PageSize;
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = m_PageSize - (readCursor % m_PageSize);
                        int toRead = Math.Min(leftToRead, spaceInPage);
                        Array.Copy(currentPage, (readCursor % m_PageSize), result, result.Length - leftToRead, toRead);
                        leftToRead -= toRead;
                        readCursor += toRead;
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                
                return null;
            }
        }

        /// <summary>
        /// Peeking is like reading but without discarding,
        /// if you will call peek or read again, you will get the same data.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public byte[] Peek(int offset, int size)
        {
            try
            {
                lock (m_Locker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    byte[] result = new byte[size];
                    int readCursor = m_ReadCursor + offset;
                    int leftToRead = size;
                    while (leftToRead != 0)
                    {
                        int currentPageIndex = readCursor / m_PageSize;
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = m_PageSize - (readCursor % m_PageSize);
                        int toRead = Math.Min(leftToRead, spaceInPage);
                        Array.Copy(currentPage, (readCursor % m_PageSize), result, result.Length - leftToRead, toRead);
                        leftToRead -= toRead;
                        readCursor += toRead;
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                
                return null;
            }
        }

        /// <summary>
        /// Clears this buffer.
        /// </summary>
        public void Clear()
        {
            try
            {
                lock (m_Locker)
                {
                    m_ReadCursor = 0;
                    m_WriteCursor = 0;
                    m_Length = 0;
                    mPages.Clear();
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void AddPage()
        {
            try
            {
                lock (m_Locker)
                {
                    mPages.Add(new byte[m_PageSize]);
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void Purge()
        {
            try
            {
                lock (m_Locker)
                {
                    int readPageIndex = m_ReadCursor / m_PageSize;
                    while (readPageIndex > 0)
                    {
                        mPages.RemoveAt(0);
                        m_ReadCursor -= m_PageSize;
                        m_WriteCursor -= m_PageSize;
                        m_Length = m_WriteCursor - m_ReadCursor;
                        readPageIndex = m_ReadCursor / m_PageSize;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
