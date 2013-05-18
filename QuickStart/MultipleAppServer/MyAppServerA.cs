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

        protected override void OnStarted()
        {
            m_DespatchServer = this.Bootstrap.GetServerByName("ServerB") as IDespatchServer;
            base.OnStarted();
        }

        internal void DespatchMessage(string targetSessionKey, string message)
        {
            m_DespatchServer.DispatchMessage(targetSessionKey, message);
        }
    }
}
