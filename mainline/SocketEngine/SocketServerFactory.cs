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

        public ISocketServer CreateSocketServer<TAppSession, TCommandInfo>(IAppServer<TAppSession> appServer, System.Net.IPEndPoint localEndPoint, SocketBase.Config.IServerConfig config, ICustomProtocol<TCommandInfo> protocol)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : SocketBase.Command.ICommandInfo
        {
            if (protocol == null)
                throw new ArgumentNullException("protocol");

            switch(config.Mode)
            {
                case(SocketMode.Udp):
                    return new UdpSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol);
                case(SocketMode.Sync):
                    return new SyncSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol);
                case(SocketMode.Async):
                    if (string.IsNullOrEmpty(config.Security) || config.Security.Equals(m_SecurityNone, StringComparison.OrdinalIgnoreCase))
                        return new AsyncSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol);
                    else
                        return new AsyncStreamSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol);
                default:
                    throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
            }
        }

        #endregion
    }
}
