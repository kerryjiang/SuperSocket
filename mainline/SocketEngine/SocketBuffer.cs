using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    public class SocketBuffer
    {
        private object mLocker = new object();
        private Int32 mPageSize = 1024;
        private Int32 mReadCursor = 0;
        private Int32 mWriteCursor = 0;
        private Int32 mLength = 0;
        private List<byte[]> mPages = new List<byte[]>();


        /// <summary>Returns the length of valid content.</summary>
        public int Length
        {// No need to lock this field. It's updated when locked whenever this SocketBuffer changes.
            get { return mLength; }
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
                mPageSize = pageSize;
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
                lock (mLocker)
                {
                    if (data == null || length == 0)
                    {
                        return;
                    }
                    int leftToWrite = length;
                    while (leftToWrite != 0)
                    {
                        int currentPageIndex = mWriteCursor / mPageSize;
                        if (currentPageIndex >= mPages.Count)
                        {
                            AddPage();
                        }
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = mPageSize - (mWriteCursor % mPageSize);
                        int toWrite = Math.Min(leftToWrite, spaceInPage);
                        Array.Copy(data, start, currentPage, (mWriteCursor % mPageSize), toWrite);
                        leftToWrite -= toWrite;
                        mWriteCursor += toWrite;
                        start += toWrite;
                        mLength = mWriteCursor - mReadCursor;
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
                lock (mLocker)
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
                lock (mLocker)
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
                lock (mLocker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    mReadCursor += size;
                    mLength = mWriteCursor - mReadCursor;
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
                lock (mLocker)
                {
                    if ((startIndex + size) > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    mReadCursor += startIndex + size;
                    mLength = mWriteCursor - mReadCursor;
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
                lock (mLocker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    byte[] result = new byte[size];
                    int readCursor = mReadCursor;
                    int leftToRead = size;
                    while (leftToRead != 0)
                    {
                        int currentPageIndex = readCursor / mPageSize;
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = mPageSize - (readCursor % mPageSize);
                        int toRead = Math.Min(leftToRead, spaceInPage);
                        Array.Copy(currentPage, (readCursor % mPageSize), result, result.Length - leftToRead, toRead);
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

        /// <summary>Peeking is like reading but without discarding, 
        /// if you will call peek or read again, you will get the same data.</summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] Peek(int offset, int size)
        {
            try
            {
                lock (mLocker)
                {
                    if (size > Length)
                    {
                        throw new ArgumentOutOfRangeException("Size is larger then buffer content");
                    }
                    byte[] result = new byte[size];
                    int readCursor = mReadCursor + offset;
                    int leftToRead = size;
                    while (leftToRead != 0)
                    {
                        int currentPageIndex = readCursor / mPageSize;
                        byte[] currentPage = mPages[currentPageIndex];
                        int spaceInPage = mPageSize - (readCursor % mPageSize);
                        int toRead = Math.Min(leftToRead, spaceInPage);
                        Array.Copy(currentPage, (readCursor % mPageSize), result, result.Length - leftToRead, toRead);
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

        public void Clear()
        {
            try
            {
                lock (mLocker)
                {
                    mReadCursor = 0;
                    mWriteCursor = 0;
                    mLength = 0;
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
                lock (mLocker)
                {
                    mPages.Add(new byte[mPageSize]);
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
                lock (mLocker)
                {
                    int readPageIndex = mReadCursor / mPageSize;
                    while (readPageIndex > 0)
                    {
                        mPages.RemoveAt(0);
                        mReadCursor -= mPageSize;
                        mWriteCursor -= mPageSize;
                        mLength = mWriteCursor - mReadCursor;
                        readPageIndex = mReadCursor / mPageSize;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        SocketBuffer f = new SocketBuffer();

    //        byte[] buffer = new byte[1100];
    //        for( int i = 0; i < buffer.Length; ++i )
    //        {
    //            buffer[i] = (byte)i;
    //        }
    //        f.Write(buffer, 0, buffer.Length);
    //        System.Diagnostics.Debug.Write("SocketBuffer.SocketBuffer == " + f.Length);
    //        
    //    }   byte[] result2 = f.Read(900, 200);
    //}
}
