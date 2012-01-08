using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public class CommandLineRequestFilterFactory : IRequestFilterFactory<StringRequestInfo>
    {
        private Encoding m_Encoding;
        private byte[] m_LineTerminator;
        private IRequestInfoParser<StringRequestInfo> m_LineParser;

        public CommandLineRequestFilterFactory()
            : this(Encoding.ASCII, new BasicRequestInfoParser())
        {

        }

        public CommandLineRequestFilterFactory(Encoding encoding, IRequestInfoParser<StringRequestInfo> lineParser)
        {
            m_Encoding = encoding;
            m_LineTerminator = encoding.GetBytes("\r\n");
            m_LineParser = lineParser;
        }

        public virtual IRequestFilter<StringRequestInfo> CreateFilter(IAppServer appServer)
        {
            return new TerminatorRequestFilter(m_LineTerminator, m_Encoding, m_LineParser);
        }
    }
}
