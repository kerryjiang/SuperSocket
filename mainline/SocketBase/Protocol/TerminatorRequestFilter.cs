using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    public abstract class TerminatorRequestFilter<TRequestInfo> : RequestFilterBase<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private SearchMarkState<byte> m_SearchState;
        private static readonly TRequestInfo m_NullRequestInfo = default(TRequestInfo);

        public TerminatorRequestFilter(byte[] terminator)
        {
            m_SearchState = new SearchMarkState<byte>(terminator);
        }

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

        protected abstract TRequestInfo Resolve(ArraySegmentList data);
    }

    public class TerminatorRequestFilter : TerminatorRequestFilter<StringRequestInfo>
    {
        private Encoding m_Encoding;
        private IRequestInfoParser<StringRequestInfo> m_RequestParser;

        public TerminatorRequestFilter(byte[] terminator, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestParser)
            : base(terminator)
        {
            m_Encoding = encoding;
            m_RequestParser = requestParser;
        }

        protected override StringRequestInfo Resolve(ArraySegmentList data)
        {
            return m_RequestParser.ParseRequestInfo(data.Decode(m_Encoding));
        }
    }
}
