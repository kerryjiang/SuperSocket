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

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        public CommandLineProtocol()
            : this(new BasicCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineProtocol"/> class.
        /// </summary>
        /// <param name="commandParser">The command parser.</param>
        public CommandLineProtocol(ICommandParser commandParser)
        {
            m_CommandParser = commandParser;
        }

        /// <summary>
        /// Creates the command reader.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public ICommandReader<StringCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new TerminatorCommandReader(appServer, Encoding.UTF8, Environment.NewLine, m_CommandParser);
        }
    }
}
