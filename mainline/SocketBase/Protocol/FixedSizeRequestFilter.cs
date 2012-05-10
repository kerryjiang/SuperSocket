using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// FixedSizeRequestFilter
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class FixedSizeRequestFilter<TRequestInfo> : IRequestFilter<TRequestInfo>, IOffsetAdapter
        where TRequestInfo : IRequestInfo
    {
        private int m_ParsedLength;

        private int m_Size;

        /// <summary>
        /// Gets the size of the fixed size request filter.
        /// </summary>
        public int Size
        {
            get { return m_Size; }
        }

        private static TRequestInfo m_NullRequestInfo = default(TRequestInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeRequestFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public FixedSizeRequestFilter(int size)
        {
            m_Size = size;
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
        public TRequestInfo Filter(IAppSession<TRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            if (m_ParsedLength + length >= m_Size)
            {
                m_OffsetDelta = 0 - m_ParsedLength;
                return FilterBuffer(session, readBuffer, offset - m_ParsedLength, m_ParsedLength + length, toBeCopied, out left);
            }
            else
            {
                m_ParsedLength += length;
                m_OffsetDelta = length;
                left = 0;
                return m_NullRequestInfo;
            }
        }

        /// <summary>
        /// Filters the buffer after the server receive the enough size of data.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left.</param>
        /// <returns></returns>
        protected abstract TRequestInfo FilterBuffer(IAppSession<TRequestInfo> session, byte[] buffer, int offset, int length, bool toBeCopied, out int left);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize { get; private set; }

        /// <summary>
        /// Gets the next request filter.
        /// </summary>
        public virtual IRequestFilter<TRequestInfo> NextRequestFilter
        {
            get { return null; }
        }


        private int m_OffsetDelta;

        /// <summary>
        /// Gets the offset delta.
        /// </summary>
        int IOffsetAdapter.OffsetDelta
        {
            get
            {
                return m_OffsetDelta;
            }
        }
    }
}
