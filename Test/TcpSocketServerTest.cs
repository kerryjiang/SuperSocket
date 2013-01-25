using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Test
{
    [TestFixture]
    public class TcpSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "TestServer.config";
            }
        }
    }
}
