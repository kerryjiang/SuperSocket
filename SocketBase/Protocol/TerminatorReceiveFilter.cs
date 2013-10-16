using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Terminator Receive filter
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class TerminatorReceiveFilter<TRequestInfo> : ReceiveFilterBase<TRequestInfo>, IOffsetAdapter, IReceiveFilterInitializer
        where TRequestInfo : IRequestInfo
    {
        private readonly SearchMarkState<byte> m_SearchState;

        private IAppSession m_Session;

        /// <summary>
        /// Gets the session assosiated with the Receive filter.
        /// </summary>
        protected IAppSession Session
        {
            get { return m_Session; }
        }

        /// <summary>
        /// Null RequestInfo
        /// </summary>
        protected static readonly TRequestInfo NullRequestInfo = default(TRequestInfo);

        private int m_ParsedLengthInBuffer = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        protected TerminatorReceiveFilter(byte[] terminator)
        {
            m_SearchState = new SearchMarkState<byte>(terminator);
        }

        void IReceiveFilterInitializer.Initialize(IAppServer appServer, IAppSession session)
        {
            m_Session = session;
        }

        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest, the length of the data which hasn't been parsed.</param>
        /// <returns>return the parsed TRequestInfo</returns>
        public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;

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

                    if (m_ParsedLengthInBuffer >= m_Session.Config.ReceiveBufferSize)
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
            var currentMatched = m_SearchState.Mark.Length - prevMatched;

            //The prev matched part is not belong to the current matched terminator mark
            if (prevMatched > 0 && findLen != 0)
            {
                //rest prevMatched to 0
                prevMatched = 0;
                currentMatched = m_SearchState.Mark.Length;
            }

            rest = length - findLen - currentMatched;

            TRequestInfo requestInfo;

            if (findLen > 0)
            {
                if(this.BufferSegments != null && this.BufferSegments.Count > 0)
                {
                    this.AddArraySegment(readBuffer, offset - m_ParsedLengthInBuffer, findLen + m_ParsedLengthInBuffer, toBeCopied);
                    requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
                }
                else
                {
                    requestInfo = ProcessMatchedRequest(readBuffer, offset - m_ParsedLengthInBuffer, findLen + m_ParsedLengthInBuffer);
                }
            }
            else if (prevMatched > 0)
            {
                if (m_ParsedLengthInBuffer > 0)
                {
                    if (m_ParsedLengthInBuffer < prevMatched)
                    {
                        BufferSegments.TrimEnd(prevMatched - m_ParsedLengthInBuffer);
                        requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
                    }
                    else
                    {
                        if (this.BufferSegments != null && this.BufferSegments.Count > 0)
                        {
                            this.AddArraySegment(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer - prevMatched, toBeCopied);
                            requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
                        }
                        else
                        {
                            requestInfo = ProcessMatchedRequest(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer - prevMatched);
                        }
                    }
                }
                else
                {
                    BufferSegments.TrimEnd(prevMatched);
                    requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
                }
            }
            else
            {
                if (this.BufferSegments != null && this.BufferSegments.Count > 0)
                {
                    if (m_ParsedLengthInBuffer > 0)
                    {
                        this.BufferSegments.AddSegment(readBuffer, offset, m_ParsedLengthInBuffer);
                    }

                    requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
                }
                else
                {
                    requestInfo = ProcessMatchedRequest(readBuffer, offset - m_ParsedLengthInBuffer, m_ParsedLengthInBuffer);
                }
            }

            InternalReset();

            if(rest == 0)
            {
                m_OffsetDelta = 0;
            }
            else
            {
                m_OffsetDelta += (length - rest);
            }

            return requestInfo;
        }

        private void InternalReset()
        {
            m_ParsedLengthInBuffer = 0;
            m_SearchState.Matched = 0;
            base.Reset();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            InternalReset();
            m_OffsetDelta = 0;
        }

        
        private TRequestInfo ProcessMatchedRequest(ArraySegmentList data, int offset, int length)
        {
            var targetData = data.ToArrayData(offset, length);
            return ProcessMatchedRequest(targetData, 0, length);
        }

        /// <summary>
        /// Resolves the specified data to TRequestInfo.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected abstract TRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length);

        
        private int m_OffsetDelta;

        int IOffsetAdapter.OffsetDelta
        {
            get { return m_OffsetDelta; }
        }
    }

    /// <summary>
    /// TerminatorRequestFilter
    /// </summary>
    public class TerminatorReceiveFilter : TerminatorReceiveFilter<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly IRequestInfoParser<StringRequestInfo> m_RequestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        public TerminatorReceiveFilter(byte[] terminator, Encoding encoding)
            : this(terminator, encoding, BasicRequestInfoParser.DefaultInstance)
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="requestParser">The request parser.</param>
        public TerminatorReceiveFilter(byte[] terminator, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestParser)
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
        protected override StringRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length)
        {
            if(length == 0)
                return m_RequestParser.ParseRequestInfo(string.Empty);

            return m_RequestParser.ParseRequestInfo(m_Encoding.GetString(data, offset, length));
        }
    }
}
