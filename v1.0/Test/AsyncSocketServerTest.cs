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
    public class AsyncSocketServerTest : SocketServerTest
    {
        protected override IServerConfig DefaultServerConfig
        {
            get
            {
                return new ServerConfig
                    {
                        Ip = "Any",
                        LogCommand = true,
                        MaxConnectionNumber = 3,
                        Mode = SocketMode.Async,
                        Name = "Async Test Socket Server",
                        Port = 100
                    };
            }
        }
    }
}
