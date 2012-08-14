using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Terminator RequestFilter Factory
    /// </summary>
    public class TerminatorRequestFilterFactory : IRequestFilterFactory<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly byte[] m_Terminator;
        private readonly IRequestInfoParser<StringRequestInfo> m_RequestInfoParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        public TerminatorRequestFilterFactory(string terminator)
            : this(terminator, Encoding.ASCII, BasicRequestInfoParser.DefaultInstance)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        public TerminatorRequestFilterFactory(string terminator, Encoding encoding)
            : this(terminator, encoding, BasicRequestInfoParser.DefaultInstance)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="requestInfoParser">The line parser.</param>
        public TerminatorRequestFilterFactory(string terminator, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestInfoParser)
        {
            m_Encoding = encoding;
            m_Terminator = encoding.GetBytes(terminator);
            m_RequestInfoParser = requestInfoParser;
        }

        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>
        /// the new created request filer assosiated with this socketSession
        /// </returns>
        public virtual IRequestFilter<StringRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new TerminatorRequestFilter(m_Terminator, m_Encoding, m_RequestInfoParser);
        }
    }
}
