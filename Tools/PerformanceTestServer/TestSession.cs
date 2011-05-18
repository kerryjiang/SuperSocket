using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace PerformanceTestServer
{
    public class TestSession : AppSession<TestSession>
    {
        public new TestServer AppServer
        {
            get
            {
                return (TestServer)base.AppServer;
            }
        }


        public override void HandleUnknownCommand(StringCommandInfo cmdInfo)
        {
            this.Logger.LogError(this, string.Format("Unknonw commamnd {0} is found!", cmdInfo.Key));
        }
    }
}
