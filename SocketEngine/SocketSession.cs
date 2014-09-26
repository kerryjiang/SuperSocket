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
        public const int Closed = 16777216;//256 * 256 * 256; 0x01 0x00 0x00 0x00
        public const int InSending = 1;//0000 0001  > 1
        public const int InReceiving = 2;//0000 0010 > 2
        public const int InSendingReceivingMask = -4;// ~(InSending | InReceiving); 0xf0 0xff 0xff 0xff
    }

    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract partial class SocketSession : ISocketSession
    {
        public IAppSession AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        //0x00 0x00 0x00 0x00
        //1st byte: Closed(Y/N) - 0x01
        //2nd byte: N/A
        //3th byte: CloseReason
        //Last byte: 0000 0000 - normal state
        //0000 0001: in sending
        //0000 0010: in receiving
        //0001 0000: in closing
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
            //Already closed
            if (!TryAddStateFlag(SocketState.Closed))
                return;

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
            if (IsClosed)
                return false;

            var queue = m_SendingQueue;

            if (queue == null)
                return false;

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
                    OnSendEnd();
                    return;
                }
            }

            Socket client;

            if (IsInClosingOrClosed && TryValidateClosedBySocket(out client))
            {
                OnSendEnd(true);
                return;
            }

            SendingQueue newQueue;

            if (!m_SendingQueuePool.TryGet(out newQueue))
            {
                AppSession.Logger.Error("There is no enougth sending queue can be used.");
                OnSendEnd(false);
                this.Close(CloseReason.InternalError);
                return;
            }

            var oldQueue = Interlocked.CompareExchange(ref m_SendingQueue, newQueue, queue);

            if (!ReferenceEquals(oldQueue, queue))
            {
                if (newQueue != null)
                    m_SendingQueuePool.Push(newQueue);

                if (IsInClosingOrClosed)
                {
                    OnSendEnd(true);
                }
                else
                {
                    OnSendEnd(false);
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
                OnSendEnd(false);
                this.Close(CloseReason.InternalError);
                return;
            }

            Send(queue);
        }

        private void OnSendEnd()
        {
            OnSendEnd(IsInClosingOrClosed);
        }

        private void OnSendEnd(bool isInClosingOrClosed)
        {
            RemoveStateFlag(SocketState.InSending);

            if (isInClosingOrClosed)
            {
                Socket client;

                if (!TryValidateClosedBySocket(out client))
                {
                    var sendingQueue = m_SendingQueue;
                    //No data to be sent
                    if (sendingQueue != null && sendingQueue.Count == 0)
                    {
                        if (client != null)// the socket instance is not closed yet, do it now
                            InternalClose(client, GetCloseReasonFromState(), false);
                        else// The UDP mode, the socket instance always is null, fire the closed event directly
                            OnClosed(GetCloseReasonFromState());

                        return;
                    }

                    return;
                }

                if (ValidateNotInSendingReceiving())
                {
                    FireCloseEvent();
                }
            }
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);

            var newQueue = m_SendingQueue;

            if (IsInClosingOrClosed)
            {
                Socket client;

                //has data is being sent and the socket isn't closed
                if (newQueue.Count > 0 && !TryValidateClosedBySocket(out client))
                {
                    StartSend(newQueue, newQueue.TrackID, false);
                    return;
                }

                OnSendEnd(true);
                return;
            }
            
            if (newQueue.Count == 0)
            {
                OnSendEnd();

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

        protected virtual bool TryValidateClosedBySocket(out Socket socket)
        {
            socket = m_Client;
            //Already closed/closing
            return socket == null;
        }

        public virtual void Close(CloseReason reason)
        {
            //Already in closing procedure
            if (!TryAddStateFlag(SocketState.InClosing))
                return;

            Socket client;

            //No need to clean the socket instance
            if (TryValidateClosedBySocket(out client))
                return;

            //Some data is in sending
            if (CheckState(SocketState.InSending))
            {
                //Set closing reason only, don't close the socket directly
                AddStateFlag(GetCloseReasonValue(reason));
                return;
            }

            // In the udp mode, we needn't close the socket instance
            if (client != null)
                InternalClose(client, reason, true);
            else //In Udp mode, and the socket is not in the sending state, then fire the closed event directly
                OnClosed(reason);
        }

        private void InternalClose(Socket client, CloseReason reason, bool setCloseReason)
        {
            if (Interlocked.CompareExchange(ref m_Client, null, client) == client)
            {
                if (setCloseReason)
                    AddStateFlag(GetCloseReasonValue(reason));

                client.SafeClose();

                if (ValidateNotInSendingReceiving())
                {
                    OnClosed(reason);
                }
            }
        }

        protected void OnSendError(SendingQueue queue, CloseReason closeReason)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);
            OnSendEnd();
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

        /// <summary>
        /// Validates the socket is not in the sending or receiving operation.
        /// </summary>
        /// <returns></returns>
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

        private int GetCloseReasonValue(CloseReason reason)
        {
            return ((int)reason + 1) * m_CloseReasonMagic;
        }

        private CloseReason GetCloseReasonFromState()
        {
            return (CloseReason)(m_State / m_CloseReasonMagic - 1);
        }

        private void FireCloseEvent()
        {
            OnClosed(GetCloseReasonFromState());
        }

        private void ValidateClosed(CloseReason closeReason)
        {
            if (IsClosed)
                return;

            if (CheckState(SocketState.InClosing))
            {
                if (ValidateNotInSendingReceiving())
                {
                    FireCloseEvent();
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
            if (socketErrorCode == 10004 //Interrupted
                || socketErrorCode == 10053 //ConnectionAborted
                || socketErrorCode == 10054 //ConnectionReset
                || socketErrorCode == 10058 //Shutdown
                || socketErrorCode == 10060 //TimedOut
                || socketErrorCode == 995 //OperationAborted
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
