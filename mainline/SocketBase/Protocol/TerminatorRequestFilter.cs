using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Terminator Request Filter
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class TerminatorRequestFilter<TRequestInfo> : RequestFilterBase<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private SearchMarkState<byte> m_SearchState;
        private static readonly TRequestInfo m_NullRequestInfo = default(TRequestInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorRequestFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        public TerminatorRequestFilter(byte[] terminator)
        {
            m_SearchState = new SearchMarkState<byte>(terminator);
        }

        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left, the length of the data which hasn't been parsed.</param>
        /// <returns>return the parsed TRequestInfo</returns>
        public override TRequestInfo Filter(IAppSession<TRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

            int prevMatched = m_SearchState.Matched;

            int result = readBuffer.SearchMark(offset, length, m_SearchState);

            if (result < 0)
            {
                this.AddArraySegment(readBuffer, offset, length, toBeCopied);
                return m_NullRequestInfo;
            }

            int findLen = result - offset;

            if (findLen > 0)
            {
                this.AddArraySegment(readBuffer, offset, findLen, false);
            }
            else if (prevMatched > 0)
            {
                BufferSegments.TrimEnd(prevMatched);
            }

            var requestInfo = Resolve(BufferSegments);

            ClearBufferSegments();

            left = length - findLen - (m_SearchState.Mark.Length - prevMatched);

            return requestInfo;
        }

        /// <summary>
        /// Resolves the specified data to TRequestInfo.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected abstract TRequestInfo Resolve(ArraySegmentList data);
    }

    /// <summary>
    /// TerminatorRequestFilter
    /// </summary>
    public class TerminatorRequestFilter : TerminatorRequestFilter<StringRequestInfo>
    {
        private Encoding m_Encoding;
        private IRequestInfoParser<StringRequestInfo> m_RequestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorRequestFilter"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="requestParser">The request parser.</param>
        public TerminatorRequestFilter(byte[] terminator, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestParser)
            : base(terminator)
        {
            m_Encoding = encoding;
            m_RequestParser = requestParser;
        }

        /// <summary>
        /// Resolves the specified data to StringRequestInfo.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected override StringRequestInfo Resolve(ArraySegmentList data)
        {
            return m_RequestParser.ParseRequestInfo(data.Decode(m_Encoding));
        }
    }
}
