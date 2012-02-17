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
                AppSession.Logger.Error(e);
                Close(CloseReason.SocketError);
                return;
            }
        }

        private void OnSessionStarting()
        {
            m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);

            if (!m_IsReset)
                StartSession();
        }

        private void OnStreamEndRead(IAsyncResult result)
        {
            var stream = result.AsyncState as Stream;

            try
            {
                int thisRead = stream.EndRead(result);

                if (thisRead > 0)
                {
                    AppSession.ProcessRequest(m_ReadBuffer, m_Offset, thisRead, true);

                    m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);
                }
            }
            catch (ObjectDisposedException)
            {
                this.Close(CloseReason.SocketError);
                return;
            }
            catch (IOException ioe)
            {
                if (ioe.InnerException != null)
                {
                    if (ioe.InnerException is SocketException)
                    {
                        var se = ioe.InnerException as SocketException;
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10053 || se.ErrorCode == 10054 || se.ErrorCode == 10058)
                        {
                            this.Close(CloseReason.SocketError);
                            return;
                        }
                    }

                    if (ioe.InnerException is ObjectDisposedException)
                    {
                        this.Close(CloseReason.SocketError);
                        return;
                    }
                }

                AppSession.Logger.Error(this, ioe);
                this.Close(CloseReason.SocketError);
                return;
            }
            catch (Exception e)
            {
                AppSession.Logger.Error(this, e);
                this.Close(CloseReason.Unknown);
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
                AppSession.Logger.Error(e);
                this.Close(CloseReason.SocketError);
                return;
            }

            m_Stream = sslStream;
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            try
            {
                m_Stream.Write(data, offset, length);
                m_Stream.Flush();
            }
            catch (Exception)
            {
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
