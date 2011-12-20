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
    public class SyncSocketServerTest : SocketServerTest
    {
        protected override IServerConfig DefaultServerConfig
        {
            get
            {
                return new ServerConfig
                    {
                        Ip = "Any",
                        LogCommand = true,
                        MaxConnectionNumber = 100,
                        Mode = SocketMode.Sync,
                        Name = "Sync Test Socket Server",
                        Port = 2012,
                        ClearIdleSession = true,
                        ClearIdleSessionInterval = 1,
                        IdleSessionTimeOut = 5
                    };
            }
        }
    }
}
