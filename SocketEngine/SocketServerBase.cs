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
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Utils;

namespace SuperSocket.SocketEngine
{
    abstract class SocketServerBase : ISocketServer, IDisposable, IAsyncSocketEventComplete
    {
        protected object SyncRoot = new object();

        public IAppServer AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        protected ListenerInfo[] ListenerInfos { get; private set; }

        protected List<ISocketListener> Listeners { get; private set; }

        protected bool IsStopped { get; set; }

        private IPool<SaeState> m_SaePool;

        protected IPool<SaeState> SaePool
        {
            get { return m_SaePool; }
        }

        private IPool<BufferState> m_BufferStatePool;

        protected IPool<BufferState> BufferStatePool
        {
            get { return m_BufferStatePool; }
        }

        /// <summary>
        /// Gets the sending queue manager.
        /// </summary>
        /// <value>
        /// The sending queue manager.
        /// </value>
        internal IPool<SendingQueue> SendingQueuePool { get; private set; }

        IPool ISocketServer.SendingQueuePool
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

            try
            {
                InitializePools();
            }
            catch (Exception e)
            {
                log.Error("Failed to initialize pools related with socket server.", e);
                return false;
            }

            try
            {
                if (!StartListeners())
                    return false;
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }

            IsRunning = true;
            return true;
        }

        private void InitializePools()
        {
            var config = AppServer.Config;

            int bufferSize = config.ReceiveBufferSize;

            if (bufferSize <= 0)
                bufferSize = 1024 * 4;

            var bufferManager = AppServer.BufferManager;

            var initialCount = Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber);

            //Plain tcp socket or udp
            if(config.Mode == SocketMode.Udp || ListenerInfos.Any(l => l.Security == SslProtocols.None))
            {
                m_SaePool = new IntelliPool<SaeState>(initialCount, new SaeStateCreator(bufferManager, bufferSize, this));
            }

            //TLS/SSL TCP
            if (ListenerInfos.Any(l => l.Security != SslProtocols.None))
            {
                m_BufferStatePool = new IntelliPool<BufferState>(initialCount, new BufferStateCreator(m_BufferStatePool, bufferManager, bufferSize));
            }

            SendingQueuePool = new IntelliPool<SendingQueue>(Math.Max(config.MaxConnectionNumber / 6, 256), new SendingQueueSourceCreator(config.SendingQueueSize));
        }

        private bool StartListeners()
        {
            ILog log = AppServer.Logger;

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

            IsRunning = false;
        }

        void IAsyncSocketEventComplete.HandleSocketEventComplete(object sender, SocketAsyncEventArgs e)
        {
            var userToken = e.UserToken as SaeState;
            var socketSession = userToken.SocketSession as IAsyncSocketSession;
            socketSession.ProcessReceive(e);
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
