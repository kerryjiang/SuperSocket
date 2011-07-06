using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace SuperSocket.QuickStart.MultipleAppServer
{
    public class MyAppServerA : AppServer
    {
        private IDespatchServer m_DespatchServer;

        protected override void OnStartup()
        {
            m_DespatchServer = SocketServerManager.GetServerByName("ServerB") as IDespatchServer;
            base.OnStartup();
        }

        internal void DespatchMessage(string targetSessionKey, string message)
        {
            m_DespatchServer.DispatchMessage(targetSessionKey, message);
        }
    }
}
