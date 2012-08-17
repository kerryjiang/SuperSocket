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
    public abstract class TerminatorRequestFilter<TRequestInfo> : RequestFilterBase<TRequestInfo>, IOffsetAdapter
        where TRequestInfo : IRequestInfo
    {
        private readonly SearchMarkState<byte> m_SearchState;

        private static readonly TRequestInfo NullRequestInfo = default(TRequestInfo);

        private int m_ParsedLengthInBuffer = 0;

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
        public override TRequestInfo Filter(IAppSession session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

            int prevMatched = m_SearchState.Matched;

            int result = readBuffer.SearchMark(offset, length, m_SearchState);

            if (result < 0)
            {
                if (m_OffsetDelta != m_ParsedLengthInBuffer)
                {
                    Buffer.BlockCopy(readBuffer, offset - m_ParsedLengthInBuffer, readBuffer, offset - m_OffsetDelta, m_ParsedLengthInBuffer + length);

                    m_ParsedLengthInBuffer += length;
                    m_OffsetDelta = m_ParsedLengthInBuffer;
                }
                else
                {
                    m_ParsedLengthInBuffer += length;

                    if (m_ParsedLengthInBuffer >= session.Config.ReceiveBufferSize)
                    {
                        this.AddArraySegment(readBuffer, offset + length - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer, toBeCopied);
                        m_ParsedLengthInBuffer = 0;
                        m_OffsetDelta = 0;

                        return NullRequestInfo;
                    }

                    m_OffsetDelta += length;
                }

                return NullRequestInfo;
            }

            var findLen = result - offset;

            left = length - findLen - (m_SearchState.Mark.Length - prevMatched);

            TRequestInfo requestInfo;

            if (findLen > 0)
            {
                if(this.BufferSegments != null && this.BufferSegments.Count > 0)
                {
                    this.AddArraySegment(readBuffer, offset - m_ParsedLengthInBuffer, findLen + m_ParsedLengthInBuffer, toBeCopied);
                    requestInfo = Resolve(BufferSegments, 0, BufferSegments.Count);
                }
                else
                {
                    requestInfo = Resolve(readBuffer, offset - m_ParsedLengthInBuffer, findLen + m_ParsedLengthInBuffer);
                }
            }
            else if (prevMatched > 0)
            {
                if (m_ParsedLengthInBuffer > 0)
                {
                    if (m_ParsedLengthInBuffer < prevMatched)
                    {
                        BufferSegments.TrimEnd(prevMatched - m_ParsedLengthInBuffer);
                        requestInfo = Resolve(BufferSegments, 0, BufferSegments.Count);
                    }
                    else
                    {
                        if (this.BufferSegments != null && this.BufferSegments.Count > 0)
                        {
                            this.AddArraySegment(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer - prevMatched, toBeCopied);
                            requestInfo = Resolve(BufferSegments, 0, BufferSegments.Count);
                        }
                        else
                        {
                            requestInfo = Resolve(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer - prevMatched);
                        }
                    }
                }
                else
                {
                    BufferSegments.TrimEnd(prevMatched);
                    requestInfo = Resolve(BufferSegments, 0, BufferSegments.Count);
                }
            }
            else
            {
                if (this.BufferSegments != null && this.BufferSegments.Count > 0)
                {
                    requestInfo = Resolve(BufferSegments, 0, BufferSegments.Count);
                }
                else
                {
                    requestInfo = Resolve(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer);
                }
            }

            Reset();

            if(left == 0)
            {
                m_OffsetDelta = 0;
            }
            else
            {
                m_OffsetDelta += (length - left);
            }

            return requestInfo;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            m_ParsedLengthInBuffer = 0;
            base.Reset();
        }

        /// <summary>
        /// Resolves the specified data to TRequestInfo.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected abstract TRequestInfo Resolve(IList<byte> data, int offset, int length);

        
        private int m_OffsetDelta;

        int IOffsetAdapter.OffsetDelta
        {
            get { return m_OffsetDelta; }
        }
    }

    /// <summary>
    /// TerminatorRequestFilter
    /// </summary>
    public class TerminatorRequestFilter : TerminatorRequestFilter<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly IRequestInfoParser<StringRequestInfo> m_RequestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorRequestFilter"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        public TerminatorRequestFilter(byte[] terminator, Encoding encoding)
            : this(terminator, encoding, BasicRequestInfoParser.DefaultInstance)
        {
            
        }
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
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected override StringRequestInfo Resolve(IList<byte> data, int offset, int length)
        {
            if(length == 0)
                return m_RequestParser.ParseRequestInfo(string.Empty);

            var segmentList = data as ArraySegmentList;

            if (segmentList != null)
                return m_RequestParser.ParseRequestInfo(((ArraySegmentList)data).Decode(m_Encoding, offset, length));

            var array = data as byte[];

            if (array != null)
                return m_RequestParser.ParseRequestInfo(m_Encoding.GetString(array, offset, length));

            return m_RequestParser.ParseRequestInfo(m_Encoding.GetString(data.CloneRange(offset, length)));
        }
    }
}
