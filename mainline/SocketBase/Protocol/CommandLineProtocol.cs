using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public class CommandLineProtocol : SocketProtocolBase, ISyncProtocol<StringCommandInfo>, IAsyncProtocol<StringCommandInfo>
    {
        private ICommandParser m_CommandParser;

        public CommandLineProtocol() : this(new BasicCommandParser())
        {

        }

        public CommandLineProtocol(ICommandParser commandParser)
        {
            m_CommandParser = commandParser;
        }

        #region IAsyncProtocol Members

        public ICommandAsyncReader<StringCommandInfo> CreateAsyncCommandReader()
        {
            return new TerminatorCommandAsyncReader(Encoding.UTF8, Environment.NewLine, m_CommandParser);
        }

        #endregion

        #region ISyncProtocol Members

        public ICommandStreamReader<StringCommandInfo> CreateSyncCommandReader()
        {
            return new TerminatorCommandStreamReader(Environment.NewLine, m_CommandParser);
        }

        #endregion
    }
}
