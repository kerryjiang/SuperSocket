using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket;

namespace SuperSocket.Management.Server
{
    public class ManagementServer : WebSocketServer<ManagementSession>
    {
        private string m_ManagePassword;

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<WebSocketRequestInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            var password = config.Options.GetValue("managePassword");

            if (string.IsNullOrEmpty(password))
            {
                Logger.Error("managePassword is required in configuration!");
                return false;
            }

            m_ManagePassword = password;

            return true;
        }
    }
}
