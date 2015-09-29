using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// TerminatorReceiveFilter with StringPackageInfo as package info
    /// </summary>
    public class TerminatorReceiveFilter : TerminatorReceiveFilter<StringPackageInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly IStringParser m_StringParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter" /> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="stringParser">The string parser.</param>
        public TerminatorReceiveFilter(byte[] terminator, Encoding encoding, IStringParser stringParser)
            : base(terminator)
        {
            m_Encoding = encoding;
            m_StringParser = stringParser;
        }

        /// <summary>
        /// Resolves the package.
        /// </summary>
        /// <param name="bufferStream">The received buffer stream.</param>
        /// <returns></returns>
        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var encoding = m_Encoding;
            var totalLen = (int)bufferStream.Length - SearchState.Mark.Length;
            return new StringPackageInfo(bufferStream.ReadString(totalLen, encoding), m_StringParser);
        }
    }
}
