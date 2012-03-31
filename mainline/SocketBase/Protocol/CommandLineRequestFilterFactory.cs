using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// CommandLineRequestFilterFactory
    /// </summary>
    public class CommandLineRequestFilterFactory : IRequestFilterFactory<StringRequestInfo>
    {
        private Encoding m_Encoding;
        private byte[] m_LineTerminator;
        private IRequestInfoParser<StringRequestInfo> m_LineParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        public CommandLineRequestFilterFactory()
            : this(Encoding.ASCII, new BasicRequestInfoParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="lineParser">The line parser.</param>
        public CommandLineRequestFilterFactory(Encoding encoding, IRequestInfoParser<StringRequestInfo> lineParser)
        {
            m_Encoding = encoding;
            m_LineTerminator = encoding.GetBytes("\r\n");
            m_LineParser = lineParser;
        }

        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>the new created request filer assosiated with this socketSession</returns>
        public virtual IRequestFilter<StringRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new TerminatorRequestFilter(m_LineTerminator, m_Encoding, m_LineParser);
        }
    }
}
