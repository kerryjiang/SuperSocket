using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;

namespace PerformanceTestServer
{
    public class TestServer : AppServer<TestSession>
    {
        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<StringCommandInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            string sendResponse = config.Options.GetValue("sendResponse", "false");
            
            bool sendResponseValue = false;
            bool.TryParse(sendResponse, out sendResponseValue);

            SendResponseToClient = sendResponseValue;
            return true;
        }

        internal bool SendResponseToClient { get; private set; }
    }
}
