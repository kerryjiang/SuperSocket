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
    static class SocketState
    {
        public const int Normal = 0;//0000 0000
        public const int InClosing = 16;//0001 0000  >= 16
        public const int Closed = 8 * 16;//1000 0000 > 128
        public const int InSending = 1;//0000 0001  > 1
        public const int InReceiving = 2;//0000 0010 > 2
        public const int InSendingReceivingMask = -251658241;// 0xff 0xff 0xff 0xf0
    }

    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract partial class SocketSession : ISocketSession
    {
        public IAppSession AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        //0x00 0x00 0x00 0x00
        //Last byte: 0000 0000
        //bit 7: closed 
        //bit 6: in sending
        //bit 5: in receiving
        private int m_State = 0;

        private void AddStateFlag(int stateValue)
        {
            while(true)
            {
                var oldState = m_State;
                var newState = m_State | stateValue;

                if(Interlocked.CompareExchange(ref m_State, newState, oldState) == oldState)
                    return;
            }
        }

        private bool TryAddStateFlag(int stateValue)
        {
            while (true)
            {
                var oldState = m_State;
                var newState = m_State | stateValue;

                //Already marked
                if (oldState == newState)
                {
                    return false;
                }

                var compareState = Interlocked.CompareExchange(ref m_State, newState, oldState);

                if (compareState == oldState)
                    return true;
            }
        }

        private void RemoveStateFlag(int stateValue)
        {
            while(true)
            {
                var oldState = m_State;
                var newState = m_State & (~stateValue);

                if(Interlocked.CompareExchange(ref m_State, newState, oldState) == oldState)
                    return;
            }
        }

        private bool CheckState(int stateValue)
        {
            return (m_State & stateValue) == stateValue;
        }

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
            m_SendingQueuePool = ((SocketServerBase)((ISocketServerAccessor)appSession.AppServer).SocketServer).SendingQueuePool;

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
        protected virtual void OnClosed(CloseReason reason)
        {
            AddStateFlag(SocketState.Closed);

            //Before changing m_SendingQueue, must check m_IsClosed
            while (true)
            {
                var sendingQueue = m_SendingQueue;

                if (sendingQueue == null)
                    break;

                //There is no sending was started after the m_Closed ws set to 'true'
                if (Interlocked.CompareExchange(ref m_SendingQueue, null, sendingQueue) == sendingQueue)
                {
                    sendingQueue.Clear();
                    m_SendingQueuePool.Push(sendingQueue);
                    break;
                }
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
            if (IsClosed)
                return false;

            var queue = m_SendingQueue;

            if (queue == null)
                return false;

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
                if (!TryAddStateFlag(SocketState.InSending))
                {
                    return;
                }

                var currentQueue = m_SendingQueue;

                if (currentQueue != queue || sendingTrackID != currentQueue.TrackID)
                {
                    //Has been sent
                    RemoveStateFlag(SocketState.InSending);
                    return;
                }
            }

            if (IsInClosingOrClosed)
                return;

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

                RemoveStateFlag(SocketState.InSending);

                if (!IsInClosingOrClosed)
                {
                    AppSession.Logger.Error("Failed to switch the sending queue.");
                    this.Close(CloseReason.InternalError);
                }

                return;
            }

            //Start to allow enqueue
            newQueue.StartEnqueue();
            queue.StopEnqueue();

            if (queue.Count == 0)
            {
                AppSession.Logger.Error("There is no data to be sent in the queue.");
                m_SendingQueuePool.Push(queue);
                RemoveStateFlag(SocketState.InSending);
                this.Close(CloseReason.InternalError);
                return;
            }

            Send(queue);
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);

            if (IsInClosingOrClosed)
            {
                RemoveStateFlag(SocketState.InSending);
                return;
            }

            var newQueue = m_SendingQueue;

            if (newQueue.Count == 0)
            {
                RemoveStateFlag(SocketState.InSending);

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

        protected bool IsInClosingOrClosed
        {
            get { return m_State >= SocketState.InClosing; }
        }

        protected bool IsClosed
        {
            get { return m_State >= SocketState.Closed; }
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
            if (!TryAddStateFlag(SocketState.InClosing))
                return;

            var client = m_Client;

            //Already closed/closing
            if (client == null)
                return;

            if (Interlocked.CompareExchange(ref m_Client, null, client) == client)
            {
                AddStateFlag(((int)reason + 1) * m_CloseReasonMagic);
                client.SafeClose();

                if (ValidateNotInSendingReceiving())
                    OnClosed(reason);
            }
        }

        protected void OnSendError(SendingQueue queue, CloseReason closeReason)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);
            RemoveStateFlag(SocketState.InSending);
            ValidateClosed(closeReason);
        }

        protected void OnReceiveError(CloseReason closeReason)
        {
            OnReceiveEnded();
            ValidateClosed(closeReason);
        }

        protected void OnReceiveStarted()
        {
            AddStateFlag(SocketState.InReceiving);
        }

        protected void OnReceiveEnded()
        {
            RemoveStateFlag(SocketState.InReceiving);
        }

        private bool ValidateNotInSendingReceiving()
        {
            var oldState = m_State;

            if ((oldState & SocketState.InSendingReceivingMask) == oldState)
            {
                return true;
            }

            return false;
        }

        private const int m_CloseReasonMagic = 256;

        private void ValidateClosed(CloseReason closeReason)
        {
            if (IsClosed)
                return;

            if (CheckState(SocketState.InClosing))
            {
                if (ValidateNotInSendingReceiving())
                {
                    OnClosed((CloseReason)(m_State / m_CloseReasonMagic - 1));
                }
            }
            else
            {
                Close(closeReason);
            }
        }

        public abstract int OrigReceiveOffset { get; }

        protected virtual bool IsIgnorableSocketError(int socketErrorCode)
        {
            if (socketErrorCode == 10004
                || socketErrorCode == 10053
                || socketErrorCode == 10054
                || socketErrorCode == 10058
                || socketErrorCode == 995
                || socketErrorCode == -1073741299)
            {
                return true;
            }

            return false;
        }

        protected virtual bool IsIgnorableException(Exception e, out int socketErrorCode)
        {
            socketErrorCode = 0;

            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            SocketException socketException = null;

            if (e is IOException)
            {
                if (e.InnerException is ObjectDisposedException || e.InnerException is NullReferenceException)
                    return true;

                socketException = e.InnerException as SocketException;
            }
            else
            {
                socketException = e as SocketException;
            }

            if (socketException == null)
                return false;

            socketErrorCode = socketException.ErrorCode;

            if (Config.LogAllSocketException)
                return false;

            return IsIgnorableSocketError(socketErrorCode);
        }
    }
}
