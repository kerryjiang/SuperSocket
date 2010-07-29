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
        bool IsRunning { get; }
        void Stop();
    }

    public abstract class SocketServerBase<TSocketSession, TAppSession> : ISocketServer, IAsyncRunner
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>, new()        
    {
        protected object SyncRoot = new object();

        public IPEndPoint EndPoint { get; private set; }

        public IAppServer<TAppSession> AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        protected bool IsStopped { get; set; }

        public SocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
        {
            AppServer = appServer;
            EndPoint = localEndPoint;
            IsRunning = false;
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

            if (AppServer.Config.ReceiveBufferSize > 0)
                client.ReceiveBufferSize = AppServer.Config.ReceiveBufferSize;

            if (AppServer.Config.SendBufferSize > 0)
                client.SendBufferSize = AppServer.Config.SendBufferSize;

            TSocketSession session = new TSocketSession();
            TAppSession appSession = this.AppServer.CreateAppSession(session);
            session.Initialize(this.AppServer, appSession, client);
            LogUtil.LogInfo(AppServer, "SocketSession " + appSession.SessionID + " was accepted!");
            return session;
        }

        protected bool VerifySocketServerRunning(bool isRunning)
        {
            //waiting 5 seconds
            int steps = 10 * 5;

            while (steps > 0)
            {
                Thread.Sleep(100);

                if (IsRunning == isRunning)
                    return true;

                steps--;
            }

            return false;
        }

        public virtual void Stop()
        {
            IsStopped = true;
        }
    }
}
