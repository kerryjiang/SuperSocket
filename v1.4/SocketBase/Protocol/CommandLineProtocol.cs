using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// It is the custom protocol implementation for command line style protocol
    /// </summary>
    public class CommandLineProtocol : ICustomProtocol<StringCommandInfo>
    {
        private ICommandParser m_CommandParser;

        private Encoding m_Encoding;

        private byte[] m_LineTerminator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        public CommandLineProtocol()
            : this(new BasicCommandParser(), Encoding.UTF8)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public CommandLineProtocol(Encoding encoding)
            : this(new BasicCommandParser(), encoding)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        /// <param name="commandParser">The command parser.</param>
        public CommandLineProtocol(ICommandParser commandParser)
            : this(commandParser, Encoding.UTF8)
        {
            m_CommandParser = commandParser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        /// <param name="commandParser">The command parser.</param>
        /// <param name="encoding">The encoding.</param>
        public CommandLineProtocol(ICommandParser commandParser, Encoding encoding)
        {
            if (commandParser == null)
                throw new ArgumentNullException("commandParser");

            m_CommandParser = commandParser;

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            m_Encoding = encoding;
            m_LineTerminator = m_Encoding.GetBytes(Environment.NewLine);
        }

        /// <summary>
        /// Creates the command reader.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public ICommandReader<StringCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new TerminatorCommandReader(appServer, m_Encoding, m_LineTerminator, m_CommandParser);
        }
    }
}
