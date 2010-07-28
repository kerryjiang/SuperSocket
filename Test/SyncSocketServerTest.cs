using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.Test
{
    [TestFixture]
    public class SyncSocketServerTest : SocketServerTest<ServerConfig>
    {
        public SyncSocketServerTest()
            : base(new ServerConfig
            {
                Ip = "Any",
                LogCommand = true,
                MaxConnectionNumber = 1,
                Mode = SocketMode.Sync,
                Name = "Sync Test Socket Server",
                Port = 100
            })

        {
 
        }
    }
}
