using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
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

        ILog IAsyncSocketSessionBase.Logger
        {
            get { return AppSession.Logger; }
        }

        public override void Start()
        {
            SocketAsyncProxy.Initialize(this);
            StartReceive(SocketAsyncProxy.SocketEventArgs);

            if (!SyncSend)
            {
                m_SocketEventArgSend = new SocketAsyncEventArgs();
                m_SocketEventArgSend.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            }

            if (!m_IsReset)
                StartSession();
        }

        bool ProcessCompleted(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            if (e.BytesTransferred <= 0)
            {
                Close(CloseReason.ClientClosing);
                return false;
            }

            if (e.SocketError != SocketError.Success)
            {
                if (e.SocketError != SocketError.ConnectionAborted
                    && e.SocketError != SocketError.ConnectionReset
                    && e.SocketError != SocketError.Interrupted
                    && e.SocketError != SocketError.Shutdown)
                {
                    AppSession.Logger.Error(AppSession, new SocketException((int)e.SocketError));
                }

                Close(CloseReason.SocketError);
                return false;
            }

            return true;
        }

        void OnSendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (!ProcessCompleted(e))
                return;

            base.OnSendingCompleted();
        }

        private bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException)
                return true;

            if (e is SocketException)
            {
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
            if (IsClosed)
                return;

            bool willRaiseEvent = false;

            try
            {
                if (offsetDelta != 0)
                {
                    e.SetBuffer(e.Offset + offsetDelta, e.Count - offsetDelta);

                    if (e.Count > AppSession.AppServer.Config.ReceiveBufferSize)
                        throw new ArgumentException("Illigal offsetDelta", "offsetDelta");
                }

                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception exc)
            {
                if (!IsIgnorableException(exc))
                    AppSession.Logger.Error(AppSession, exc);

                Close(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        protected override void SendSync(byte[] data, int offset, int length)
        {
            try
            {
                Client.Send(data, offset, length, SocketFlags.None);
                OnSendingCompleted();
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                Close(CloseReason.SocketError);
            }
        }

        protected override void SendAsync(byte[] data, int offset, int length)
        {
            try
            {
                m_SocketEventArgSend.SetBuffer(data, offset, length);

                if (!Client.SendAsync(m_SocketEventArgSend))
                    OnSendingCompleted(Client, m_SocketEventArgSend);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                Close(CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!ProcessCompleted(e))
                return;

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
    }
}
