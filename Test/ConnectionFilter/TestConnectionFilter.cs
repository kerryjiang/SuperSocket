using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using System.Threading;

namespace SuperSocket.Test.ConnectionFilter
{
    public class TestConnectionFilter : IConnectionFilter
    {
        internal IAppServer AppServer { get; private set; }

        public bool Initialize(string name, IAppServer appServer)
        {
            Name = name;
            AppServer = appServer;
            return true;
        }

        public string Name { get; private set; }

        public static bool Allow { get; set; }

        private static int m_ConnectedCount = 0;

        public static int ConnectedCount
        {
            get { return m_ConnectedCount; }
        }

        public bool AllowConnect(System.Net.IPEndPoint remoteAddress)
        {
            if (!Allow)
                return false;

            Interlocked.Increment(ref m_ConnectedCount);
            return true;
        }
    }
}
