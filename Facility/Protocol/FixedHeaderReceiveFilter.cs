using System;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// FixedHeaderReceiveFilter,
    /// it is the Receive filter base for the protocol which define fixed length header and the header contains the request body length,
    /// you can implement your own Receive filter for this kind protocol easily by inheriting this class 
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class FixedHeaderReceiveFilter<TRequestInfo> : FixedSizeReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private bool m_FoundHeader = false;

        private ArraySegment<byte> m_Header;

        private int m_BodyLength;

        private ArraySegmentList m_BodyBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedHeaderReceiveFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="headerSize">Size of the header.</param>
        protected FixedHeaderReceiveFilter(int headerSize)
            : base(headerSize)
        {

        }

        /// <summary>
        /// Filters the specified session.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest.</param>
        /// <returns></returns>
        public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            if (!m_FoundHeader)
                return base.Filter(readBuffer, offset, length, toBeCopied, out rest);

            if (m_BodyBuffer == null || m_BodyBuffer.Count == 0)
            {
                if (length < m_BodyLength)
                {
                    if (m_BodyBuffer == null)
                        m_BodyBuffer = new ArraySegmentList();

                    m_BodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    return NullRequestInfo;
                }
                else if (length == m_BodyLength)
                {
                    rest = 0;
                    m_FoundHeader = false;
                    return ResolveRequestInfo(m_Header, readBuffer, offset, length);
                }
                else
                {
                    rest = length - m_BodyLength;
                    m_FoundHeader = false;
                    return ResolveRequestInfo(m_Header, readBuffer, offset, m_BodyLength);
                }
            }
            else
            {
                int required = m_BodyLength - m_BodyBuffer.Count;

                if (length < required)
                {
                    m_BodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    return NullRequestInfo;
                }
                else if (length == required)
                {
                    m_BodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    m_FoundHeader = false;
                    var requestInfo = ResolveRequestInfo(m_Header, m_BodyBuffer.ToArrayData());
                    m_BodyBuffer.ClearSegements();
                    return requestInfo;
                }
                else
                {
                    m_BodyBuffer.AddSegment(readBuffer, offset, required, toBeCopied);
                    rest = length - required;
                    m_FoundHeader = false;
                    var requestInfo = ResolveRequestInfo(m_Header, m_BodyBuffer.ToArrayData(0, m_BodyLength));
                    m_BodyBuffer.ClearSegements();
                    return requestInfo;
                }
            }
        }

        /// <summary>
        /// Processes the fix size request.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns></returns>
        protected override TRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
        {
            m_FoundHeader = true;

            m_BodyLength = GetBodyLengthFromHeader(buffer, offset, Size);

            if (toBeCopied)
                m_Header = new ArraySegment<byte>(buffer.CloneRange(offset, Size));
            else
                m_Header = new ArraySegment<byte>(buffer, offset, Size);

            if (m_BodyLength > 0)
                return NullRequestInfo;

            m_FoundHeader = false;
            return ResolveRequestInfo(m_Header, null, 0, 0);//Empty body
        }

        private TRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer)
        {
            return ResolveRequestInfo(header, bodyBuffer, 0, bodyBuffer.Length);
        }

        /// <summary>
        /// Gets the body length from header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected abstract int GetBodyLengthFromHeader(byte[] header, int offset, int length);

        /// <summary>
        /// Resolves the request data.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="bodyBuffer">The body buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected abstract TRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length);

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            m_FoundHeader = false;
            m_BodyLength = 0;
        }
    }
}
