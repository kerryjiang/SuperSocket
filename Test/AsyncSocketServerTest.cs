using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.Test
{
    [TestFixture]
    public class AsyncSocketServerTest : SocketServerTest
    {
        public AsyncSocketServerTest()
            : base("Async Test Socket Server", 100, SocketMode.Async)
        {

        }
    }
}
