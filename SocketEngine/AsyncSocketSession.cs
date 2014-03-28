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

namespace SuperSocket.SocketEngine
{
    class AsyncSocketSession : SocketSession, IAsyncSocketSession, IBufferRecycler
    {
        private bool m_IsReset;

        private IPool<SocketAsyncEventArgs> m_SaePoolForReceive;

        private SocketAsyncEventArgs m_SocketEventArgSend;

        public AsyncSocketSession(Socket client, IPool<SocketAsyncEventArgs> saePoolForReceive)
            : this(client, saePoolForReceive, false)
        {

        }

        public AsyncSocketSession(Socket client, IPool<SocketAsyncEventArgs> saePoolForReceive, bool isReset)
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
            var sae = m_SaePoolForReceive.Get();
            sae.UserToken = this;
            StartReceive(sae);

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
            if (!ProcessCompleted(e))
            {
                m_SaePoolForReceive.Return(e);
                OnReceiveError(CloseReason.ClientClosing);
                return;
            }

            OnReceiveEnded();

            var state = ProcessReceivedData(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred), e);

            if (state == ProcessState.Pending)
            {
                e = m_SaePoolForReceive.Get();
                e.UserToken = this;
            }

            //read the next block of data sent from the client
            StartReceive(e);
        }

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }

        protected override void ReturnBuffer(IList<KeyValuePair<ArraySegment<byte>, object>> buffers, int offset, int length)
        {
            SocketAsyncEventArgs prev = null;

            for (var i = 0; i < length; i++)
            {
                var current = buffers[offset + i].Value as SocketAsyncEventArgs;

                if (current != prev)
                {
                    m_SaePoolForReceive.Return(current);
                }

                prev = current;
            }
        }
    }
}
