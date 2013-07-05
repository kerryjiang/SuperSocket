using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Receive filter base class
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class ReceiveFilterBase<TRequestInfo> : IReceiveFilter<TRequestInfo>
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
        /// Initializes a new instance of the <see cref="ReceiveFilterBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        protected ReceiveFilterBase()
        {
            m_BufferSegments = new ArraySegmentList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveFilterBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="previousRequestFilter">The previous Receive filter.</param>
        protected ReceiveFilterBase(ReceiveFilterBase<TRequestInfo> previousRequestFilter)
        {
            Initialize(previousRequestFilter);
        }

        /// <summary>
        /// Initializes the specified previous Receive filter.
        /// </summary>
        /// <param name="previousRequestFilter">The previous Receive filter.</param>
        public void Initialize(ReceiveFilterBase<TRequestInfo> previousRequestFilter)
        {
            m_BufferSegments = previousRequestFilter.BufferSegments;
        }

        #region IReceiveFilter<TRequestInfo> Members


        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest, the length of the data which hasn't been parsed.</param>
        /// <returns></returns>
        public abstract TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest);


        /// <summary>
        /// Gets the size of the rest buffer.
        /// </summary>
        /// <value>
        /// The size of the rest buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_BufferSegments.Count; }
        }

        /// <summary>
        /// Gets or sets the next Receive filter.
        /// </summary>
        /// <value>
        /// The next Receive filter.
        /// </value>
        public IReceiveFilter<TRequestInfo> NextReceiveFilter { get; protected set; }

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

        /// <summary>
        /// Resets this instance to initial state.
        /// </summary>
        public virtual void Reset()
        {
            if(m_BufferSegments != null && m_BufferSegments.Count > 0)
                m_BufferSegments.ClearSegements();
        }

        /// <summary>
        /// Gets the filter state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public FilterState State { get; protected set; }
    }
}
