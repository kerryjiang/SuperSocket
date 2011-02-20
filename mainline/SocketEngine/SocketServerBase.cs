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
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    abstract class SocketServerBase<TSocketSession, TAppSession> : ISocketServer, IDisposable
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>
    {
        protected object SyncRoot = new object();

        public IPEndPoint EndPoint { get; private set; }

        public IAppServer<TAppSession> AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        protected bool IsStopped { get; set; }

        private ManualResetEvent m_ServerStartupEvent = new ManualResetEvent(false);

        public SocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint)
        {
            AppServer = appServer;
            EndPoint = localEndPoint;
            IsRunning = false;
        }

        public virtual bool Start()
        {
            IsStopped = false;
            m_ServerStartupEvent.Reset();
            return true;
        }

        protected void WaitForStartupFinished()
        {
            m_ServerStartupEvent.WaitOne();
        }

        protected void OnStartupFinished()
        {
            m_ServerStartupEvent.Set();
        }        

        protected bool VerifySocketServerRunning(bool isRunning)
        {
            //waiting 10 seconds
            int steps = 10 * 10;

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

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                m_ServerStartupEvent.Close();
        }

        #endregion
    }
}
