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
using SuperSocket.SocketBase.Sockets;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketSession : SocketSession, IAsyncSocketSession
    {
        private bool m_IsReset;

        private ISocketAsyncEventArgs m_SocketEventArgSend;
        private ISocketFactory m_SocketFactory;

        public AsyncSocketSession(ISocket client, SocketAsyncEventArgsProxy socketAsyncProxy, ISocketFactory socketFactory)
            : this(client, socketAsyncProxy, false, socketFactory)
        {

        }

        public AsyncSocketSession(ISocket client, SocketAsyncEventArgsProxy socketAsyncProxy, bool isReset, ISocketFactory socketFactory)
            : base(client)
        {
            SocketAsyncProxy = socketAsyncProxy;
            m_IsReset = isReset;
            m_SocketFactory = socketFactory;
        }

        ILog ILoggerProvider.Logger
        {
            get { return AppSession.Logger; }
        }

        public override void Initialize(IAppSession appSession)
        {
            base.Initialize(appSession);

            //Initialize SocketAsyncProxy for receiving
            SocketAsyncProxy.Initialize(this);

            if (!SyncSend)
            {
                //Initialize SocketAsyncEventArgs for sending
                m_SocketEventArgSend = m_SocketFactory.CreateSocketAsyncEventArgs();
                m_SocketEventArgSend.Completed += new EventHandler<ISocketAsyncEventArgs>(OnSendingCompleted);
            }
        }

        public override void Start()
        {
            StartReceive(SocketAsyncProxy.SocketEventArgs);

            if (!m_IsReset)
                StartSession();
        }

        bool ProcessCompleted(ISocketAsyncEventArgs e)
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
                LogError((int)e.SocketError);
            }

            return false;
        }

        void OnSendingCompleted(object sender, ISocketAsyncEventArgs e)
        {
            var queue = e.UserToken as SendingQueue;

            if (!ProcessCompleted(e))
            {
                ClearPrevSendState(e);
                OnSendError(queue, CloseReason.SocketError);
                return;
            }

            var count = queue.Sum(q => q.Count);

            if (count != e.BytesTransferred)
            {
                queue.InternalTrim(e.BytesTransferred);
                AppSession.Logger.InfoFormat("{0} of {1} were transferred, send the rest {2} bytes right now.", e.BytesTransferred, count, queue.Sum(q => q.Count));
                ClearPrevSendState(e);
                SendAsync(queue);
                return;
            }

            ClearPrevSendState(e);
            base.OnSendingCompleted(queue);
        }

        private void ClearPrevSendState(ISocketAsyncEventArgs e)
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

        private void StartReceive(ISocketAsyncEventArgs e)
        {
            StartReceive(e, 0);
        }

        private void StartReceive(ISocketAsyncEventArgs e, int offsetDelta)
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

                // the connection is closing or closed
                if (!OnReceiveStarted())
                    return;

                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception exc)
            {
                LogError(exc);
                OnReceiveTerminated(CloseReason.SocketError);
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
                LogError(e);

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
                LogError(e);

                ClearPrevSendState(m_SocketEventArgSend);
                OnSendError(queue, CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        public void ProcessReceive(ISocketAsyncEventArgs e)
        {
            if (!ProcessCompleted(e))
            {
                OnReceiveTerminated(e.SocketError == SocketError.Success ? CloseReason.ClientClosing : CloseReason.SocketError);
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
                LogError("Protocol error", exc);
                this.Close(CloseReason.ProtocolError);
                return;
            }

            //read the next block of data sent from the client
            StartReceive(e, offsetDelta);
        }

        protected override void OnClosed(CloseReason reason)
        {
            var sae = m_SocketEventArgSend;

            if (sae == null)
            {
                base.OnClosed(reason);
                return;
            }

            if (Interlocked.CompareExchange(ref m_SocketEventArgSend, null, sae) == sae)
            {
                sae.Dispose();
                base.OnClosed(reason);
            }
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
