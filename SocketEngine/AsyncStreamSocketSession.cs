using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

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
            var e = socketAsyncProxy.SocketEventArgs;
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
                    if (Config.LogAllSocketException)
                        return false;

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
                OnReceiveStarted();
                m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                OnReceiveError(CloseReason.SocketError);
                return;
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

                OnReceiveError(CloseReason.SocketError);
                return;
            }

            if (thisRead <= 0)
            {
                OnReceiveError(CloseReason.ClientClosing);
                return;
            }

            OnReceiveEnded();

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
                if (offsetDelta < 0 || offsetDelta >= Config.ReceiveBufferSize)
                    throw new ArgumentException(string.Format("Illigal offsetDelta: {0}", offsetDelta), "offsetDelta");

                m_Offset = SocketAsyncProxy.OrigOffset + offsetDelta;
                m_Length = Config.ReceiveBufferSize - offsetDelta;

                OnReceiveStarted();
                m_Stream.BeginRead(m_ReadBuffer, m_Offset, m_Length, OnStreamEndRead, m_Stream);
            }
            catch (Exception exc)
            {
                if (!IsIgnorableException(exc))
                    AppSession.Logger.Error(AppSession, exc);

                OnReceiveError(CloseReason.SocketError);
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
            catch (IOException)
            {
                this.Close(CloseReason.SocketError);
                return;
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                this.Close(CloseReason.SocketError);
                return;
            }

            m_Stream = sslStream;
        }

        protected override void SendSync(SendingQueue queue)
        {
            try
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    var item = queue[i];
                    m_Stream.Write(item.Array, item.Offset, item.Count);
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

        protected override void OnSendingCompleted(SendingQueue queue)
        {
            try
            {
                m_Stream.Flush();
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                OnSendError(queue, CloseReason.SocketError);
                return;
            }

            base.OnSendingCompleted(queue);
        }

        protected override void SendAsync(SendingQueue queue)
        {
            try
            {
                var item = queue[queue.Position];
                m_Stream.BeginWrite(item.Array, item.Offset, item.Count, OnEndWrite, queue);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                OnSendError(queue, CloseReason.SocketError);
            }
        }

        private void OnEndWrite(IAsyncResult result)
        {
            var queue = result.AsyncState as SendingQueue;

            try
            {
                m_Stream.EndWrite(result);
            }
            catch (Exception e)
            {
                if (!IsIgnorableException(e))
                    AppSession.Logger.Error(AppSession, e);

                OnSendError(queue, CloseReason.SocketError);
                return;
            }
            
            var nextPos = queue.Position + 1;

            //Has more data to send
            if (nextPos < queue.Count)
            {
                queue.Position = nextPos;
                SendAsync(queue);
                return;
            }

            OnSendingCompleted(queue);
        }

        public override void ApplySecureProtocol()
        {
            var asyncResult = BeginInitStream(OnBeginInitStream);

            if (asyncResult != null)
                asyncResult.AsyncWaitHandle.WaitOne();
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        ILog ILoggerProvider.Logger
        {
            get { return AppSession.Logger; }
        }

        public override int OrigReceiveOffset
        {
            get { return SocketAsyncProxy.OrigOffset; }
        }
    }
}
