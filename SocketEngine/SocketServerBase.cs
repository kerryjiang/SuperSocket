using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    abstract class SocketServerBase : ISocketServer, IDisposable
    {
        protected object SyncRoot = new object();

        public IAppServer AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        protected ListenerInfo[] ListenerInfos { get; private set; }

        protected List<ISocketListener> Listeners { get; private set; }

        protected bool IsStopped { get; set; }

        /// <summary>
        /// Gets the sending queue manager.
        /// </summary>
        /// <value>
        /// The sending queue manager.
        /// </value>
        internal ISmartPool<SendingQueue> SendingQueuePool { get; private set; }

        IPoolInfo ISocketServer.SendingQueuePool
        {
            get { return this.SendingQueuePool; }
        }

        public SocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
        {
            AppServer = appServer;
            IsRunning = false;
            ListenerInfos = listeners;
            Listeners = new List<ISocketListener>(listeners.Length);
        }

        public abstract void ResetSessionSecurity(IAppSession session, SslProtocols security);

        public virtual bool Start()
        {
            IsStopped = false;

            ILog log = AppServer.Logger;

            var config = AppServer.Config;

            var sendingQueuePool = new SmartPool<SendingQueue>();
            sendingQueuePool.Initialize(Math.Max(config.MaxConnectionNumber / 6, 256),
                    Math.Max(config.MaxConnectionNumber * 2, 256),
                    new SendingQueueSourceCreator(config.SendingQueueSize));

            SendingQueuePool = sendingQueuePool;

            for (var i = 0; i < ListenerInfos.Length; i++)
            {
                var listener = CreateListener(ListenerInfos[i]);
                listener.Error += new ErrorHandler(OnListenerError);
                listener.Stopped += new EventHandler(OnListenerStopped);
                listener.NewClientAccepted += new NewClientAcceptHandler(OnNewClientAccepted);

                if (listener.Start(AppServer.Config))
                {
                    Listeners.Add(listener);

                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Listener ({0}) was started", listener.EndPoint);
                    }
                }
                else //If one listener failed to start, stop started listeners
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Listener ({0}) failed to start", listener.EndPoint);
                    }

                    for (var j = 0; j < Listeners.Count; j++)
                    {
                        Listeners[j].Stop();
                    }

                    Listeners.Clear();
                    return false;
                }
            }

            IsRunning = true;
            return true;
        }

        protected abstract void OnNewClientAccepted(ISocketListener listener, Socket client, object state);

        void OnListenerError(ISocketListener listener, Exception e)
        {
            var logger = this.AppServer.Logger;

            if(!logger.IsErrorEnabled)
                return;

            logger.Error(string.Format("Listener ({0}) error: {1}", listener.EndPoint, e.Message), e);
        }

        void OnListenerStopped(object sender, EventArgs e)
        {
            var listener = sender as ISocketListener;

            ILog log = AppServer.Logger;

            if (log.IsDebugEnabled)
                log.DebugFormat("Listener ({0}) was stoppped", listener.EndPoint);
        }

        protected abstract ISocketListener CreateListener(ListenerInfo listenerInfo);

        public virtual void Stop()
        {
            IsStopped = true;

            for (var i = 0; i < Listeners.Count; i++)
            {
                var listener = Listeners[i];

                listener.Stop();
            }

            Listeners.Clear();

            SendingQueuePool = null;

            IsRunning = false;
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
            {
                if (IsRunning)
                    Stop();
            }
        }

        #endregion
    }
}
