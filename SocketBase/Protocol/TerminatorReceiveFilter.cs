using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
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
        /// Resolves the package.
        /// </summary>
        /// <param name="packageData">The package data.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var maxCharsLen = m_Encoding.GetMaxCharCount(packageData.Sum(s => s.Count));

            var chars = new char[maxCharsLen];

            var decoder = m_Encoding.GetDecoder();
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;
            var output = 0;

            for (var i = 0; i < packageData.Count; i++)
            {
                var segment = packageData[i];
                decoder.Convert(segment.Array, segment.Offset, segment.Count, chars, output, maxCharsLen - output, true, out bytesUsed, out charsUsed, out completed);
                output += charsUsed;
            }

            return m_RequestParser.ParseRequestInfo(new string(chars, 0, output));
        }
    }
}
