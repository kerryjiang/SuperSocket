using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Request filter base class
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilterBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        protected RequestFilterBase()
        {
            m_BufferSegments = new ArraySegmentList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilterBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="previousRequestFilter">The previous request filter.</param>
        protected RequestFilterBase(RequestFilterBase<TRequestInfo> previousRequestFilter)
        {
            Initialize(previousRequestFilter);
        }

        /// <summary>
        /// Initializes the specified previous request filter.
        /// </summary>
        /// <param name="previousRequestFilter">The previous request filter.</param>
        public void Initialize(RequestFilterBase<TRequestInfo> previousRequestFilter)
        {
            m_BufferSegments = previousRequestFilter.BufferSegments;
        }

        #region IRequestFilter<TRequestInfo> Members


        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left, the length of the data which hasn't been parsed.</param>
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
