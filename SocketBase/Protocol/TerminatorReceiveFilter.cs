using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// TerminatorReceiveFilter with StringRequestInfo as package info
    /// </summary>
    public class TerminatorReceiveFilter : TerminatorReceiveFilter<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly IStringPackageParser<StringRequestInfo> m_PackageParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="packageParser">The package parser.</param>
        public TerminatorReceiveFilter(byte[] terminator, Encoding encoding, IStringPackageParser<StringRequestInfo> packageParser)
            : base(terminator)
        {
            m_Encoding = encoding;
            m_PackageParser = packageParser;
        }

        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="packageData">The package binary data.</param>
        /// <returns>the resolved package instance</returns>
        public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var encoding = m_Encoding;
            var totalLen = packageData.Sum(x => x.Count) - SearchState.Mark.Length;
            var charsBuffer = new char[encoding.GetMaxCharCount(totalLen)];

            int bytesUsed, charsUsed, totalBytesUsed = 0;
            bool completed;

            var decoder = encoding.GetDecoder();

            var outputOffset = 0;

            foreach (var segment in packageData)
            {
                var srcLen = Math.Min(totalLen - totalBytesUsed, segment.Count);

                if (srcLen == 0)
                    break;

                decoder.Convert(segment.Array, segment.Offset, srcLen, charsBuffer, outputOffset, charsBuffer.Length - outputOffset, true, out bytesUsed, out charsUsed, out completed);
                outputOffset += charsUsed;
                totalBytesUsed += bytesUsed;
            }

            return m_PackageParser.Parse(new string(charsBuffer, 0, outputOffset));
        }
    }
}
