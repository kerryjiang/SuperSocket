using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Utils;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketSession : SocketSession, IAsyncSocketSession
    {
        private bool m_IsReset;

        private IPool<SaeState> m_SaePoolForReceive;

        private SocketAsyncEventArgs m_SocketEventArgSend;

        public AsyncSocketSession(Socket client, IPool<SaeState> saePoolForReceive)
            : this(client, saePoolForReceive, false)
        {

        }

        public AsyncSocketSession(Socket client, IPool<SaeState> saePoolForReceive, bool isReset)
            : base(client)
        {
            m_SaePoolForReceive = saePoolForReceive;
            m_IsReset = isReset;
        }

        public override void Initialize(IAppSession appSession)
        {
            base.Initialize(appSession);

            if (!SyncSend)
            {
                //Initialize SocketAsyncEventArgs for sending
                m_SocketEventArgSend = new SocketAsyncEventArgs();
                m_SocketEventArgSend.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            }
        }

        public override void Start()
        {
            var saeState = m_SaePoolForReceive.Get();
            saeState.SocketSession = this;
            saeState.Sae.UserToken = saeState;

            StartReceive(saeState.Sae);

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
                LogError((int)e.SocketError);
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


        private void StartReceive(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = false;

            try
            {
                if (IsInClosingOrClosed)
                    return;

                OnReceiveStarted();
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception exc)
            {
                LogError(exc);

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

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            var state = e.UserToken as SaeState;

            if (!ProcessCompleted(e))
            {
                e.UserToken = null;
                m_SaePoolForReceive.Return(state);
                OnReceiveError(CloseReason.ClientClosing);
                return;
            }

            OnReceiveEnded();

            var result = ProcessReceivedData(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred), state);

            if (result.State == ProcessState.Cached)
            {
                e.UserToken = null;
                var newState = m_SaePoolForReceive.Get();
                e = newState.Sae;
                e.UserToken = newState;
                newState.SocketSession = this;
            }

            //read the next block of data sent from the client
            StartReceive(e);
        }

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }

        protected override void OnClosed(CloseReason reason)
        {
            if (m_SocketEventArgSend != null)
            {
                m_SocketEventArgSend.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
                m_SocketEventArgSend.Dispose();
                m_SocketEventArgSend = null;
            }

            base.OnClosed(reason);
        }

        protected override void ReturnBuffer(IList<KeyValuePair<ArraySegment<byte>, IBufferState>> buffers, int offset, int length)
        {
            for (var i = 0; i < length; i++)
            {
                var buffer = buffers[offset + i];
                var state = buffer.Value as SaeState;

                if (state != null && state.DecreaseReference() == 0)
                {
                    m_SaePoolForReceive.Return(state);
                }
            }
        }
    }
}
