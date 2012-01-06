using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

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

        public ISocketServer CreateSocketServer<TAppSession, TRequestInfo>(IAppServer<TAppSession> appServer, System.Net.IPEndPoint localEndPoint, SocketBase.Config.IServerConfig config, ICustomProtocol<TRequestInfo> protocol)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : SocketBase.Command.IRequestInfo
        {
            if (protocol == null)
                throw new ArgumentNullException("protocol");

            switch(config.Mode)
            {
                case(SocketMode.Udp):
                    return new UdpSocketServer<TAppSession, TRequestInfo>(appServer, localEndPoint, protocol);
                case(SocketMode.Tcp):
                case(SocketMode.Async):
                    if (string.IsNullOrEmpty(config.Security) || config.Security.Equals(m_SecurityNone, StringComparison.OrdinalIgnoreCase))
                        return new AsyncSocketServer<TAppSession, TRequestInfo>(appServer, localEndPoint, protocol);
                    else
                        return new AsyncStreamSocketServer<TAppSession, TRequestInfo>(appServer, localEndPoint, protocol);
                default:
                    throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
            }
        }

        #endregion
    }
}
