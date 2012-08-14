using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// CommandLine RequestFilter Factory
    /// </summary>
    public class CommandLineRequestFilterFactory : TerminatorRequestFilterFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        public CommandLineRequestFilterFactory()
            : this(Encoding.ASCII)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public CommandLineRequestFilterFactory(Encoding encoding)
            : this(encoding, new BasicRequestInfoParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="requestInfoParser">The request info parser.</param>
        public CommandLineRequestFilterFactory(Encoding encoding, IRequestInfoParser<StringRequestInfo> requestInfoParser)
            : base("\r\n", encoding, requestInfoParser)
        {

        }
    }
}
