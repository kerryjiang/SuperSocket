using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.Common.Logging;

namespace SuperSocket.Test
{
    public class TestServerWithCustomRequestFilter : TestServer
    {
        public TestServerWithCustomRequestFilter()
            : base(new TestRequestParser())
        {

        }
    }

    public class TestServer : AppServer<TestSession>, ITestSetup
    {
        public TestServer()
            : base()
        {

        }

        public TestServer(IRequestInfoParser<StringRequestInfo> requestInfoParser)
            : base(new CommandLineRequestFilterFactory(Encoding.UTF8, requestInfoParser))
        {
            
        }

        void ITestSetup.Setup(IRootConfig rootConfig, IServerConfig serverConfig)
        {
            base.Setup(rootConfig, serverConfig, SocketServerFactory.Instance, null, new ConsoleLogFactory(), null);
        }
    }
}
