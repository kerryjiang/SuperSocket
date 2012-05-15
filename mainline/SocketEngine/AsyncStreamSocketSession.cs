using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;
using SuperSocket.Common.Logging;

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketSession : SocketSession, IAsyncSocketSessionBase
    {
        private byte[] m_ReadBuffer;
        private int m_Offset;
        private int m_Length;

        private bool m_IsReset;

        public AsyncStreamSocketSession(Socket client, SslProtocols security, SocketAsyncEventArgsProxy socketAsyncProxy)
            : this(client, security, socketAsyncProxy, false)
        {

        }

        public AsyncStreamSocketSession(Socket client, SslProtocols security, SocketAsyncEventArgsProxy socketAsyncProxy, bool isReset)
            : base(client)
        {
            SecureProtocol = security;
            SocketAsyncProxy = socketAsyncProxy;
            SocketAsyncEventArgs e = socketAsyncProxy.SocketEventArgs;
            m_ReadBuffer = e.Buffer;
            m_Offset = e.Offset;
            m_Length = e.Count;

            m_IsReset = isReset;
        }

        private bool IsIgnorableException(Exception e)
        {
            if (e is ObjectDisposedException)
                return true;

            if (e is IOException)
            {
                if (e.InnerException is ObjectDisposedException)
                    return true;

                if (e.InnerException is SocketException)
                {
                    var se = e.InnerException as SocketException;

                    if (se.ErrorCode == 10004 || se.ErrorCode == 10053 || se.ErrorCode == 10054 || se.ErrorCode == 10058 || se.ErrorCode == -1073741299)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts this session communication.
        /// </summary>
        public override void Start()
        {
            //Hasn't started, but already closed
            if (IsClosed)
                return;

            try
            {
                var asyncResult = BeginInitStream(OnBeginInitStreamOnSessionStarted);

                //If the operation is synchronous
                if (asyncResult == null)
                    OnSessionStarting();
            }
            catch (Exception e)
            {
                if(!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                Close(CloseReason.SocketError);
                return;
            }
        }

        private void OnSessionStarting()
        {
            try
            {
                m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                this.Close(CloseReason.SocketError);
            }

            if (!m_IsReset)
                StartSession();
        }

        private void OnStreamEndRead(IAsyncResult result)
        {
            var stream = result.AsyncState as Stream;

            int thisRead = 0;

            try
            {
                thisRead = stream.EndRead(result);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                this.Close(CloseReason.SocketError);
                return;
            }

            if (thisRead <= 0)
            {
                this.Close(CloseReason.ClientClosing);
                return;
            }

            int offsetDelta;

            try
            {
                offsetDelta = AppSession.ProcessRequest(m_ReadBuffer, m_Offset, thisRead, true);
            }
            catch (Exception ex)
            {
                AppSession.Logger.Error(AppSession, "protocol error", ex);
                this.Close(CloseReason.ProtocolError);
                return;
            }

            try
            {
                if (offsetDelta != 0)
                {
                    m_Offset += offsetDelta;
                    m_Length -= offsetDelta;

                    if (m_Length > AppSession.AppServer.Config.ReceiveBufferSize)
                        throw new Exception("Illigal offsetDelta");
                }

                m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);
            }
            catch (Exception exc)
            {
                if (!IsIgnorableException(exc))
                    AppSession.Logger.Error(AppSession, exc);

                this.Close(CloseReason.SocketError);
                return;
            }
        }

        private Stream m_Stream;

        private IAsyncResult BeginInitStream(AsyncCallback asyncCallback)
        {
            IAsyncResult result = null;

            switch (SecureProtocol)
            {
                case (SslProtocols.Default):
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                    SslStream sslStream = new SslStream(new NetworkStream(Client), false);
                    result = sslStream.BeginAuthenticateAsServer(AppSession.AppServer.Certificate, false, SslProtocols.Default, false, asyncCallback, sslStream);
                    break;
                case (SslProtocols.Ssl2):
                    SslStream ssl2Stream = new SslStream(new NetworkStream(Client), false);
                    result = ssl2Stream.BeginAuthenticateAsServer(AppSession.AppServer.Certificate, false, SslProtocols.Ssl2, false, asyncCallback, ssl2Stream);
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }

            return result;
        }

        private void OnBeginInitStreamOnSessionStarted(IAsyncResult result)
        {
            OnBeginInitStream(result);

            if (m_Stream != null)
                OnSessionStarting();
        }

        private void OnBeginInitStream(IAsyncResult result)
        {
            var sslStream = result.AsyncState as SslStream;

            try
            {
                sslStream.EndAuthenticateAsServer(result);
            }
            catch (Exception e)
            {
                if(!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                this.Close(CloseReason.SocketError);
                return;
            }

            m_Stream = sslStream;
        }

        protected override void SendSync(byte[] data, int offset, int length)
        {
            try
            {
                m_Stream.Write(data, offset, length);
                m_Stream.Flush();
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
                m_Stream.BeginWrite(data, offset, length, OnEndWrite, null);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                Close(CloseReason.SocketError);
            }
        }

        private void OnEndWrite(IAsyncResult result)
        {
            try
            {
                m_Stream.EndWrite(result);
                OnSendingCompleted();
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                Close(CloseReason.SocketError);
            }
        }

        public override void ApplySecureProtocol()
        {
            var asyncResult = BeginInitStream(OnBeginInitStream);

            if (asyncResult != null)
                asyncResult.AsyncWaitHandle.WaitOne();
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        ILog IAsyncSocketSessionBase.Logger
        {
            get { return AppSession.Logger; }
        }
    }
}
