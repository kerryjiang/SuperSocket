using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    public abstract class RequestFilterBase<TRequestInfo> : IRequestFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private ArraySegmentList m_BufferSegments;

        /// <summary>
        /// Gets the buffer segments which can help you parse your request info conviniently.
        /// </summary>
        protected ArraySegmentList BufferSegments
        {
            get { return m_BufferSegments; }
        }

        protected RequestFilterBase()
        {
            m_BufferSegments = new ArraySegmentList();
        }

        protected RequestFilterBase(RequestFilterBase<TRequestInfo> previousRequestFilter)
        {
            Initialize(previousRequestFilter);
        }

        public void Initialize(RequestFilterBase<TRequestInfo> previousRequestFilter)
        {
            m_BufferSegments = previousRequestFilter.BufferSegments;
        }

        #region IRequestFilter<TRequestInfo> Members

        /// <summary>
        /// Filters the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">if set to <c>true</c> [left].</param>
        /// <returns></returns>
        public abstract TRequestInfo Filter(IAppSession<TRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left);


        /// <summary>
        /// Gets the left buffer.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        protected byte[] GetLeftBuffer()
        {
            return m_BufferSegments.ToArrayData();
        }

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_BufferSegments.Count; }
        }

        /// <summary>
        /// Gets or sets the next request filter.
        /// </summary>
        /// <value>
        /// The next request filter.
        /// </value>
        public IRequestFilter<TRequestInfo> NextRequestFilter { get; protected set; }

        #endregion

        /// <summary>
        /// Adds the array segment.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        protected void AddArraySegment(byte[] buffer, int offset, int length, bool toBeCopied)
        {
            m_BufferSegments.AddSegment(buffer, offset, length, toBeCopied);
        }

        /// <summary>
        /// Clears the buffer segments.
        /// </summary>
        protected void ClearBufferSegments()
        {
            m_BufferSegments.ClearSegements();
        }
    }
}
