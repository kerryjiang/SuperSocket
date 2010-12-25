using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public class CommandLineProtocol : ICustomProtocol<StringCommandInfo>
    {
        private ICommandParser m_CommandParser;

        public CommandLineProtocol() : this(new BasicCommandParser())
        {

        }

        public CommandLineProtocol(ICommandParser commandParser)
        {
            m_CommandParser = commandParser;
        }

        public ICommandReader<StringCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new TerminatorCommandReader(appServer, Encoding.UTF8, Environment.NewLine, m_CommandParser);
        }
    }
}
