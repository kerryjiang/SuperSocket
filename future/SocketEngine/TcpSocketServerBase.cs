using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using System.Net;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    class TcpSocketServerBase<TSocketSession, TAppSession> : SocketServerBase<TSocketSession, TAppSession>
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>
    {
        public TcpSocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
            : base(appServer, localEndPoint)
        {

        }

        protected TSocketSession RegisterSession(Socket client, TSocketSession session)
        {
            //load socket setting
            if (AppServer.Config.ReadTimeOut > 0)
                client.ReceiveTimeout = AppServer.Config.ReadTimeOut;

            if (AppServer.Config.SendTimeOut > 0)
                client.SendTimeout = AppServer.Config.SendTimeOut;

            if (AppServer.Config.ReceiveBufferSize > 0)
                client.ReceiveBufferSize = AppServer.Config.ReceiveBufferSize;

            if (AppServer.Config.SendBufferSize > 0)
                client.SendBufferSize = AppServer.Config.SendBufferSize;

            TAppSession appSession = this.AppServer.CreateAppSession(session);
            session.Initialize(this.AppServer, appSession);

            return session;
        }
    }
}
