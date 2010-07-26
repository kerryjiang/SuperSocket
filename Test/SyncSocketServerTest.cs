using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.Test
{
    [TestFixture]
    public class SyncSocketServerTest : SocketServerTest
    {
        public SyncSocketServerTest()
            : base("Sync Test Socket Server", 100, SocketMode.Sync)
        {

        }
    }
}
