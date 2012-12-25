using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract class SocketSession : ISocketSession
    {
        public IAppSession AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        private int m_InSending = 0;

        protected bool SyncSend { get; private set; }

        private ISmartPool<SendingQueue> m_SendingQueuePool;

        public SocketSession(Socket client)
            : this(Guid.NewGuid().ToString())
        {
            if (client == null)
                throw new ArgumentNullException("client");

            m_Client = client;
            LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public SocketSession(string sessionID)
        {
            SessionID = sessionID;
        }

        public virtual void Initialize(IAppSession appSession)
        {
            AppSession = appSession;
            Config = appSession.Config;
            SyncSend = Config.SyncSend;
            m_SendingQueuePool = ((TcpSocketServerBase)((ISocketServerAccessor)appSession.AppServer).SocketServer).SendingQueuePool;

            SendingQueue queue;
            if (m_SendingQueuePool.TryGet(out queue))
            {
                m_SendingQueue = queue;
                queue.StartEnqueue();
            }
        }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID { get; private set; }


        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public IServerConfig Config { get; set; }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void StartSession()
        {
            AppSession.StartSession();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose(CloseReason reason)
        {
            m_IsClosed = true;
            m_InSending = 0;
            
            var queue = m_SendingQueue;

            if (queue != null)
            {
                queue.StopEnqueue();
                m_SendingQueuePool.Push(queue);
            }

            var closedHandler = Closed;
            if (closedHandler != null)
            {
                closedHandler(this, reason);
            }
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public Action<ISocketSession, CloseReason> Closed { get; set; }

        private SendingQueue m_SendingQueue;

        /// <summary>
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns></returns>
        public bool TrySend(IList<ArraySegment<byte>> segments)
        {
            var queue = m_SendingQueue;
            var trackID = queue.TrackID;

            if (!queue.Enqueue(segments, trackID))
                return false;

            StartSend(queue, trackID, true);
            return true;
        }

        /// <summary>
        /// Tries to send array segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public bool TrySend(ArraySegment<byte> segment)
        {
            var queue = m_SendingQueue;
            var trackID = queue.TrackID;

            if (!queue.Enqueue(segment, trackID))
                return false;

            StartSend(queue, trackID, true);
            return true;
        }

        /// <summary>
        /// Sends in async mode.
        /// </summary>
        /// <param name="queue">The queue.</param>
        protected abstract void SendAsync(SendingQueue queue);

        /// <summary>
        /// Sends in sync mode.
        /// </summary>
        /// <param name="queue">The queue.</param>
        protected abstract void SendSync(SendingQueue queue);

        private void Send(SendingQueue queue)
        {
            if (SyncSend)
            {
                SendSync(queue);
            }
            else
            {
                SendAsync(queue);
            }
        }

        private void StartSend(SendingQueue queue, int sendingTrackID, bool initial)
        {
            if (initial)
            {
                if (Interlocked.CompareExchange(ref m_InSending, 1, 0) != 0)
                {
                    return;
                }

                var currentQueue = m_SendingQueue;

                if (currentQueue != queue || sendingTrackID != currentQueue.TrackID)
                {
                    m_InSending = 0;
                    return;
                }
            }

            SendingQueue newQueue;

            if (!m_SendingQueuePool.TryGet(out newQueue))
            {
                AppSession.Logger.Error("There is no enougth sending queue can be used.");
                this.Close(CloseReason.InternalError);
                return;
            }

            var oldQueue = Interlocked.CompareExchange(ref m_SendingQueue, newQueue, queue);

            if (!ReferenceEquals(oldQueue, queue))
            {
                if (newQueue != null)
                    m_SendingQueuePool.Push(newQueue);

                AppSession.Logger.Error("Failed to switch the sending queue.");
                this.Close(CloseReason.InternalError);
                return;
            }

            //Start to allow enqueue
            newQueue.StartEnqueue();
            queue.StopEnqueue();

            if (queue.Count == 0)
            {
                AppSession.Logger.Error("There is no data to be sent in the queue.");
                this.Close(CloseReason.InternalError);
                return;
            }

            Send(queue);
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);

            var newQueue = m_SendingQueue;

            if (newQueue.Count == 0)
            {
                m_InSending = 0;

                if (newQueue.Count > 0)
                {
                    StartSend(newQueue, newQueue.TrackID, true);
                }
            }
            else
            {
                StartSend(newQueue, newQueue.TrackID, false);
            }
        }

        public abstract void ApplySecureProtocol();

        public Stream GetUnderlyStream()
        {
            return new NetworkStream(Client);
        }

        private Socket m_Client;
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public Socket Client
        {
            get { return m_Client; }
        }

        private bool m_IsClosed = false;

        protected bool IsClosed
        {
            get { return m_IsClosed; }
        }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>The local end point.</value>
        public virtual IPEndPoint LocalEndPoint { get; protected set; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        public virtual void Close(CloseReason reason)
        {
            var client = m_Client;

            if(client == null)
                return;

            if (Interlocked.CompareExchange(ref m_Client, null, client) == client)
            {
                client.SafeClose();
                OnClose(reason);
            }
        }
    }
}
