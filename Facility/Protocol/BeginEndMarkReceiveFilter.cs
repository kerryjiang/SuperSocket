using System;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// ReceiveFilter for the protocol that each request has bengin and end mark
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class BeginEndMarkReceiveFilter<TRequestInfo> : ReceiveFilterBase<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private readonly SearchMarkState<byte> m_BeginSearchState;
        private readonly SearchMarkState<byte> m_EndSearchState;

        private bool m_FoundBegin = false;

        /// <summary>
        /// Null request info
        /// </summary>
        protected TRequestInfo NullRequestInfo = default(TRequestInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginEndMarkReceiveFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="beginMark">The begin mark.</param>
        /// <param name="endMark">The end mark.</param>
        protected BeginEndMarkReceiveFilter(byte[] beginMark, byte[] endMark)
        {
            m_BeginSearchState = new SearchMarkState<byte>(beginMark);
            m_EndSearchState = new SearchMarkState<byte>(endMark);
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
            rest = 0;

            if (!m_FoundBegin)
            {
                var pos = readBuffer.SearchMark(offset, length, m_BeginSearchState);

                //Don't cache invalid data
                if (pos < 0)
                    return NullRequestInfo;

                //Found start mark
                m_FoundBegin = true;

                int searchEndMarkOffset = pos + m_BeginSearchState.Mark.Length;

                //The end mark could not exist in this round received data
                if (offset + length <= searchEndMarkOffset)
                {
                    AddArraySegment(m_BeginSearchState.Mark, 0, m_BeginSearchState.Mark.Length, false);
                    return NullRequestInfo;
                }

                int searchEndMarkLength = offset + length - searchEndMarkOffset;

                var endPos = readBuffer.SearchMark(searchEndMarkOffset, searchEndMarkLength, m_EndSearchState);

                if (endPos < 0)
                {
                    AddArraySegment(readBuffer, pos, length + offset - pos, toBeCopied);
                    return NullRequestInfo;
                }

                int parsedLen = endPos - pos + m_EndSearchState.Mark.Length;
                rest = length - parsedLen;

                var requestInfo = ProcessMatchedRequest(readBuffer, pos, parsedLen);

                Reset();

                return requestInfo;
            }
            else
            {
                var endPos = readBuffer.SearchMark(offset, length, m_EndSearchState);
                //Haven't found end mark
                if (endPos < 0)
                {
                    AddArraySegment(readBuffer, offset, length, toBeCopied);
                    return NullRequestInfo;
                }

                //Found end mark
                int parsedLen = endPos - offset + m_EndSearchState.Mark.Length;
                rest = length - parsedLen;

                byte[] commandData = new byte[BufferSegments.Count + parsedLen];

                if (BufferSegments.Count > 0)
                    BufferSegments.CopyTo(commandData, 0, 0, BufferSegments.Count);

                Array.Copy(readBuffer, offset, commandData, BufferSegments.Count, parsedLen);

                var requestInfo = ProcessMatchedRequest(commandData, 0, commandData.Length);

                if (requestInfo == NullRequestInfo)
                {
                    AddArraySegment(readBuffer, offset, length, toBeCopied);
                    return NullRequestInfo;
                }

                Reset();

                return requestInfo;
            }
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
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            m_BeginSearchState.Matched = 0;
            m_EndSearchState.Matched = 0;
            m_FoundBegin = false;
            base.Reset();
        }
    }
}
