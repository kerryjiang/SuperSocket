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

        #region ISocketServerFactory Members

        public ISocketServer CreateSocketServer<TAppSession, TCommandInfo>(IAppServer<TAppSession> appServer, System.Net.IPEndPoint localEndPoint, SocketBase.Config.IServerConfig config, object protocol)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : SocketBase.Command.ICommandInfo
        {
            switch(config.Mode)
            {
                case(SocketMode.Udp):
                    return new UdpSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol as IAsyncProtocol<TCommandInfo>);
                case(SocketMode.Sync):
                    return new SyncSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol as ISyncProtocol<TCommandInfo>);
                case(SocketMode.Async):
                    return new AsyncSocketServer<TAppSession, TCommandInfo>(appServer, localEndPoint, protocol as IAsyncProtocol<TCommandInfo>);
                default:
                    throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
            }
        }

        #endregion
    }
}
