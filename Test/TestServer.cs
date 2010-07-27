using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test
{
    public class TestServer : AppServer<TestSession>
    {
        public TestServer()
        {

        }

        public TestServer(ICommandParser commandParser)
            : this(commandParser, null)
        {

        }

        public TestServer(ICommandParser commandParser, ICommandParameterParser commandParameterParser)
            : this()
        {
            CommandParser = commandParser;
            CommandParameterParser = commandParameterParser;
        }
    }
}
