using System;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// This request filter is designed for this kind protocol:
    /// each request has fixed count part which splited by a char(byte)
    /// for instance, request is defined like this "#12122#23343#4545456565#343435446#",
    /// because this request is splited into many parts by 5 '#', we can create a request filter by CountSpliterRequestFilter((byte)'#', 5)
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class CountSpliterRequestFilter<TRequestInfo> : IRequestFilter<TRequestInfo>, IOffsetAdapter
        where TRequestInfo : IRequestInfo
    {
        private int m_Total;

        private int m_SpliterFoundCount;

        private readonly byte m_Spliter;

        private readonly int m_SpliterCount;

        /// <summary>
        /// Null request info instance
        /// </summary>
        protected static readonly TRequestInfo NullRequestInfo = default(TRequestInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterRequestFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        protected CountSpliterRequestFilter(byte spliter, int spliterCount)
        {
            m_Spliter = spliter;
            m_SpliterCount = spliterCount;
        }

        /// <summary>
        /// Filters the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left.</param>
        /// <returns></returns>
        public TRequestInfo Filter(IAppSession session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            int parsedLen = 0;

            for (int i = 0; i < length; i++)
            {
                if(readBuffer[offset + i] == m_Spliter)
                {
                    m_SpliterFoundCount++;

                    if(m_SpliterFoundCount == m_SpliterCount)
                    {
                        parsedLen = i + 1;
                        break;
                    }
                }
            }

            //Not found enougth spliter
            if(parsedLen == 0)
            {
                //Move current requestInfo's offset to orginal offset
                if (OffsetDelta != m_Total)
                {
                    Buffer.BlockCopy(readBuffer, offset - m_Total, readBuffer, offset - OffsetDelta, m_Total + length);

                    m_Total += length;
                    OffsetDelta = m_Total;
                }
                else
                {
                    m_Total += length;
                    OffsetDelta += length;
                }
                
                left = 0;
                return NullRequestInfo;
            }

            left = length - parsedLen;
            var finalTotal = m_Total + parsedLen;

            var requestInfo = ProcessMatchedRequest(readBuffer, offset - m_Total, finalTotal);

            InternalReset();

            if (left == 0)
            {
                OffsetDelta = 0;
            }
            else
            {
                OffsetDelta += parsedLen;
            }

            return requestInfo;
        }

        /// <summary>
        /// Processes the matched request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected abstract TRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_Total; }
        }

        /// <summary>
        /// Gets the next request filter.
        /// </summary>
        public IRequestFilter<TRequestInfo> NextRequestFilter
        {
            get { return null; }
        }

        private void InternalReset()
        {
            m_Total = 0;
            m_SpliterFoundCount = 0;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            InternalReset();
            OffsetDelta = 0;
        }

        /// <summary>
        /// Gets the offset delta relative original receiving offset which will be used for next round receiving.
        /// </summary>
        public int OffsetDelta { get; private set; }
    }

    /// <summary>
    /// This request filter is designed for this kind protocol:
    /// each request has fixed count part which splited by a char(byte)
    /// for instance, request is defined like this "#12122#23343#4545456565#343435446#",
    /// because this request is splited into many parts by 5 '#', we can create a request filter by CountSpliterRequestFilter((byte)'#', 5)
    /// </summary>
    public class CountSpliterRequestFilter : CountSpliterRequestFilter<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;

        private readonly int m_KeyIndex;

        private readonly char m_Spliter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterRequestFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        public CountSpliterRequestFilter(byte spliter, int spliterCount)
            : this(spliter, spliterCount, Encoding.ASCII)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterRequestFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        /// <param name="encoding">The encoding.</param>
        public CountSpliterRequestFilter(byte spliter, int spliterCount, Encoding encoding)
            : this(spliter, spliterCount, encoding, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterRequestFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="keyIndex">Index of the key.</param>
        public CountSpliterRequestFilter(byte spliter, int spliterCount, Encoding encoding, int keyIndex)
            : base(spliter, spliterCount)
        {
            m_Encoding = encoding;
            m_KeyIndex = keyIndex;
            m_Spliter = (char)spliter;
        }

        /// <summary>
        /// Processes the matched request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected override StringRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            //ignore the first and the last spliter
            var body = m_Encoding.GetString(readBuffer, offset + 1, length - 2);
            var array = body.Split(m_Spliter);
            return new StringRequestInfo(array[m_KeyIndex], body, array);
        }
    }
}
