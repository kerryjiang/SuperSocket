using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace SuperSocket.Management.Server
{
    public class ManagementSession : WebSocketSession<ManagementSession>
    {
        public new ManagementServer AppServer
        {
            get { return (ManagementServer)base.AppServer; }
        }

        public bool LoggedIn { get; internal set; }
    }
}
