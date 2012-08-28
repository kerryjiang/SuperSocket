using System;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

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

        /// <summary>
        /// Null RequestInfo
        /// </summary>
        protected readonly static TRequestInfo NullRequestInfo = default(TRequestInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedSizeRequestFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        protected FixedSizeRequestFilter(int size)
        {
            m_Size = size;
        }

        /// <summary>
        /// Filters the specified session.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left.</param>
        /// <returns></returns>
        public virtual TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = m_ParsedLength + length - m_Size;

            if (left >= 0)
            {
                var requestInfo = ProcessMatchedRequest(readBuffer, offset - m_ParsedLength, m_ParsedLength + length, toBeCopied);
                InternalReset();
                return requestInfo;
            }
            else
            {
                m_ParsedLength += length;
                m_OffsetDelta = m_ParsedLength;
                left = 0;
                return NullRequestInfo;
            }
        }

        /// <summary>
        /// Filters the buffer after the server receive the enough size of data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns></returns>
        protected abstract TRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_ParsedLength; }
        }

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
            get { return m_OffsetDelta; }
        }

        private void InternalReset()
        {
            m_ParsedLength = 0;
            m_OffsetDelta = 0;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public virtual void Reset()
        {
            InternalReset();
        }
    }
}
