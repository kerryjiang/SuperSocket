using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Net;

namespace SuperSocket.SocketEngine
{
    public class SocketServerFactory : ISocketServerFactory
    {
        private static ISocketServerFactory m_Instance;

        public static ISocketServerFactory Instance
        {
            get { return m_Instance; }
        }

        static SocketServerFactory()
        {
            m_Instance = new SocketServerFactory();
        }

        private const string m_SecurityNone = "None";

        #region ISocketServerFactory Members

        public ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
            where TRequestInfo : IRequestInfo
        {
            if (requestFilterFactory == null)
                throw new ArgumentNullException("requestFilterFactory");

            switch(config.Mode)
            {
                case(SocketMode.Tcp):
                    return new AsyncSocketServer(appServer, listeners);
                default:
                    throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
            }
        }

        #endregion
    }
}
