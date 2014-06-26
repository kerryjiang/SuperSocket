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
using SuperSocket.SocketBase.ServerResource;
using SuperSocket.SocketEngine.ServerResource;
using System.Threading.Tasks;

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

        private ServerResourceItem[] m_ServerResources;

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
                using (var transaction = new LightweightTransaction())
                {
                    var config = this.AppServer.Config;

                    transaction.RegisterItem(ServerResourceItem.Create<SaePoolResource, IPool<SaeState>>(
                        (pool) => this.m_SaePool = pool, config));

                    transaction.RegisterItem(ServerResourceItem.Create<BufferStatePoolResource, IPool<BufferState>>(
                        (pool) => this.m_BufferStatePool = pool, config));

                    transaction.RegisterItem(ServerResourceItem.Create<SendingQueuePoolResource, IPool<SendingQueue>>(
                        (pool) => this.SendingQueuePool = pool, config));

                    if (!StartListeners())
                        return false;

                    m_ServerResources = transaction.Items.OfType<ServerResourceItem>().ToArray();
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }

            IsRunning = true;
            return true;
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
                log.DebugFormat("Listener ({0}) was stopped", listener.EndPoint);
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

            // Clean the attached server resources
            if (m_ServerResources != null)
            {
                Parallel.ForEach(m_ServerResources, (r) => r.Rollback());
            }

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
