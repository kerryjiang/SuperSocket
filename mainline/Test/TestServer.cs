using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test
{
    public class TestServer : AppServer<TestSession>
    {
        public TestServer()
            : base()
        {

        }

        public TestServer(ICommandParser commandParser)
            : base(new CommandLineProtocol(commandParser))
        {
            
        }
    }
}
