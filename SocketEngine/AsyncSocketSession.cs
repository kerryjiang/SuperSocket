using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketSession : SocketSession, IAsyncSocketSession
    {
        private bool m_IsReset;

        private SocketAsyncEventArgs m_SocketEventArgSend;

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy)
            : this(client, socketAsyncProxy, false)
        {

        }

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy, bool isReset)
            : base(client)
        {
            SocketAsyncProxy = socketAsyncProxy;
            m_IsReset = isReset;
        }

        ILog ILoggerProvider.Logger
        {
            get { return AppSession.Logger; }
        }

        public override void Start()
        {
            SocketAsyncProxy.Initialize(this);

            if (!SyncSend)
            {
                m_SocketEventArgSend = new SocketAsyncEventArgs();
                m_SocketEventArgSend.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            }

            StartReceive(SocketAsyncProxy.SocketEventArgs);

            if (!m_IsReset)
                StartSession();
        }

        bool ProcessCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    return true;
                }
            }
            else
            {
                if (Config.LogAllSocketException ||
                            (e.SocketError != SocketError.ConnectionAborted
                                && e.SocketError != SocketError.ConnectionReset
                                && e.SocketError != SocketError.Interrupted
                                && e.SocketError != SocketError.Shutdown
                                && e.SocketError != SocketError.OperationAborted))
                {
                    AppSession.Logger.Error(AppSession, new SocketException((int)e.SocketError));
                }
            }

            return false;
        }

        void OnSendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            var queue = e.UserToken as SendingQueue;

            if (!ProcessCompleted(e))
            {
                ClearPrevSendState(e);
                OnSendError(queue, CloseReason.SocketError);
                return;
            }

            ClearPrevSendState(e);
            base.OnSendingCompleted(queue);
        }

        private void ClearPrevSendState(SocketAsyncEventArgs e)
        {
            e.UserToken = null;

            //Clear previous sending buffer of sae to avoid memory leak
            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            else if (e.BufferList != null)
            {
                e.BufferList = null;
            }
        }

        private bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            if (e is SocketException)
            {
                if (Config.LogAllSocketException)
                    return false;

                var se = e as SocketException;

                if (se.ErrorCode == 10004 || se.ErrorCode == 10053 || se.ErrorCode == 10054 || se.ErrorCode == 10058)
                    return true;
            }

            return false;
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            StartReceive(e, 0);
        }

        private void StartReceive(SocketAsyncEventArgs e, int offsetDelta)
        {
            bool willRaiseEvent = false;

            try
            {
                if (offsetDelta < 0 || offsetDelta >= Config.ReceiveBufferSize)
                    throw new ArgumentException(string.Format("Illigal offsetDelta: {0}", offsetDelta), "offsetDelta");

                var predictOffset = SocketAsyncProxy.OrigOffset + offsetDelta;

                if (e.Offset != predictOffset)
                {
                    e.SetBuffer(predictOffset, Config.ReceiveBufferSize - offsetDelta);
                }

                if (IsInClosingOrClosed)
                    return;

                OnReceiveStarted();
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception exc)
            {
                if (!IsIgnorableException(exc))
                    AppSession.Logger.Error(AppSession, exc);

                OnReceiveError(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        protected override void SendSync(SendingQueue queue)
        {
            try
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    var item = queue[i];

                    var client = Client;

                    if (client == null)
                        return;

                    client.Send(item.Array, item.Offset, item.Count, SocketFlags.None);
                }

                OnSendingCompleted(queue);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                OnSendError(queue, CloseReason.SocketError);
                return;
            }
        }

        protected override void SendAsync(SendingQueue queue)
        {
            try
            {
                m_SocketEventArgSend.UserToken = queue;

                if (queue.Count > 1)
                    m_SocketEventArgSend.BufferList = queue;
                else
                {
                    var item = queue[0];
                    m_SocketEventArgSend.SetBuffer(item.Array, item.Offset, item.Count);
                }

                var client = Client;

                if (client == null)
                {
                    OnSendError(queue, CloseReason.SocketError);
                    return;
                }

                if (!client.SendAsync(m_SocketEventArgSend))
                    OnSendingCompleted(client, m_SocketEventArgSend);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                ClearPrevSendState(m_SocketEventArgSend);
                OnSendError(queue, CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!ProcessCompleted(e))
            {
                OnReceiveError(CloseReason.ClientClosing);
                return;
            }

            OnReceiveEnded();

            int offsetDelta;

            try
            {
                offsetDelta = this.AppSession.ProcessRequest(e.Buffer, e.Offset, e.BytesTransferred, true);
            }
            catch (Exception exc)
            {
                AppSession.Logger.Error(AppSession, "protocol error", exc);
                this.Close(CloseReason.ProtocolError);
                return;
            }

            //read the next block of data sent from the client
            StartReceive(e, offsetDelta);
        }      

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }

        public override int OrigReceiveOffset
        {
            get { return SocketAsyncProxy.OrigOffset; }
        }
    }
}
