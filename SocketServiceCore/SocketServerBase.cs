using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
    public interface ISocketServer
    {
        bool Start();
        void Stop();
    }

    public abstract class SocketServerBase<TSocketSession, TAppSession> : ISocketServer, IAsyncRunner
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>, new()        
    {
        protected object SyncRoot = new object();

        public IPEndPoint EndPoint { get; private set; }

        public IAppServer<TAppSession> AppServer { get; private set; }

        public SocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
        {
            AppServer = appServer;
            EndPoint = localEndPoint;
        }

        private Dictionary<string, TAppSession> m_SessionDict = new Dictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, TAppSession> SessionDict
        {
            get
            {
                return m_SessionDict;
            }
        }

        public virtual bool Start()
        {
            return true;
        }

        protected virtual TSocketSession RegisterSession(Socket client)
        {
            //load socket setting
            if (AppServer.Config.ReadTimeOut > 0)
                client.ReceiveTimeout = AppServer.Config.ReadTimeOut;

            if (AppServer.Config.SendTimeOut > 0)
                client.SendTimeout = AppServer.Config.SendTimeOut;

            client.ReceiveBufferSize = AppServer.Config.ReceiveBufferSize;
            client.SendBufferSize = AppServer.Config.SendBufferSize;

            TSocketSession session = new TSocketSession();
            TAppSession appSession = this.AppServer.CreateAppSession(session);
            session.Initialize(this.AppServer, appSession, client);
            LogUtil.LogInfo(AppServer, "SocketSession " + appSession.SessionID + " was accepted!");
            return session;
        }

        public virtual void Stop()
        {

        }
    }
}
