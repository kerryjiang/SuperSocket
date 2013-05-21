using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// The interface for socket session which requires negotiation before communication
    /// </summary>
    interface INegotiateSocketSession
    {
        /// <summary>
        /// Start negotiates
        /// </summary>
        void Negotiate();

        /// <summary>
        /// Gets a value indicating whether this <see cref="INegotiateSocketSession" /> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.
        /// </value>
        bool Result { get; }


        /// <summary>
        /// Gets the app session.
        /// </summary>
        /// <value>
        /// The app session.
        /// </value>
        IAppSession AppSession { get; }

        /// <summary>
        /// Occurs when [negotiate completed].
        /// </summary>
        event EventHandler NegotiateCompleted;
    }

    class AsyncStreamSocketSession : SocketSession, IAsyncSocketSessionBase, INegotiateSocketSession
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

        /// <summary>
        /// Starts this session communication.
        /// </summary>
        public override void Start()
        {
            //Hasn't started, but already closed
            if (IsClosed)
                return;

            OnSessionStarting();
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
                LogError(e);

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
                LogError(e);

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
                LogError("Protocol error", ex);
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
                LogError(exc);
                OnReceiveError(CloseReason.SocketError);
                return;
            }
        }

        private Stream m_Stream;

        private SslStream CreateSslStream(ICertificateConfig certConfig)
        {
            //Enable client certificate function only if ClientCertificateRequired is true in the configuration
            if(!certConfig.ClientCertificateRequired)
                return new SslStream(new NetworkStream(Client), false);

            //Subscribe the client validation callback
            return new SslStream(new NetworkStream(Client), false, ValidateClientCertificate);
        }

        private bool ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var session = AppSession;

            //Invoke the AppServer's method ValidateClientCertificate
            var clientCertificateValidator = session.AppServer as IRemoteCertificateValidator;

            if (clientCertificateValidator != null)
                return clientCertificateValidator.Validate(session, sender, certificate, chain, sslPolicyErrors);

            //Return the native validation result
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        private IAsyncResult BeginInitStream(AsyncCallback asyncCallback)
        {
            IAsyncResult result = null;

            var certConfig = AppSession.Config.Certificate;

            switch (SecureProtocol)
            {
                case (SslProtocols.Default):
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                    SslStream sslStream = CreateSslStream(certConfig);
                    result = sslStream.BeginAuthenticateAsServer(AppSession.AppServer.Certificate, certConfig.ClientCertificateRequired, SslProtocols.Default, false, asyncCallback, sslStream);
                    break;
                case (SslProtocols.Ssl2):
                    SslStream ssl2Stream = CreateSslStream(certConfig);
                    result = ssl2Stream.BeginAuthenticateAsServer(AppSession.AppServer.Certificate, certConfig.ClientCertificateRequired, SslProtocols.Ssl2, false, asyncCallback, ssl2Stream);
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }

            return result;
        }

        private void OnBeginInitStreamOnSessionConnected(IAsyncResult result)
        {
            OnBeginInitStream(result, true);
        }

        private void OnBeginInitStream(IAsyncResult result)
        {
            OnBeginInitStream(result, false);
        }

        private void OnBeginInitStream(IAsyncResult result, bool connect)
        {
            var sslStream = result.AsyncState as SslStream;

            try
            {
                sslStream.EndAuthenticateAsServer(result);
            }
            catch (IOException exc)
            {
                LogError(exc);

                if (!connect)//Session was already registered
                    this.Close(CloseReason.SocketError);

                OnNegotiateCompleted(false);
                return;
            }
            catch (Exception e)
            {
                LogError(e);

                if (!connect)//Session was already registered
                    this.Close(CloseReason.SocketError);

                OnNegotiateCompleted(false);
                return;
            }

            m_Stream = sslStream;
            OnNegotiateCompleted(true);
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
                LogError(e);
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
                LogError(e);
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
                LogError(e);
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
                LogError(e);
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

        private bool m_NegotiateResult = false;

        void INegotiateSocketSession.Negotiate()
        {
            IAsyncResult asyncResult;

            try
            {
                asyncResult = BeginInitStream(OnBeginInitStreamOnSessionConnected);
            }
            catch (Exception e)
            {
                LogError(e);
                OnNegotiateCompleted(false);
                return;
            }

            if (asyncResult == null)
            {
                OnNegotiateCompleted(true);
                return;
            }
        }

        bool INegotiateSocketSession.Result
        {
            get { return m_NegotiateResult; }
        }

        private EventHandler m_NegotiateCompleted;

        event EventHandler INegotiateSocketSession.NegotiateCompleted
        {
            add { m_NegotiateCompleted += value; }
            remove { m_NegotiateCompleted -= value; }
        }

        private void OnNegotiateCompleted(bool negotiateResult)
        {
            m_NegotiateResult = negotiateResult;

            //One time event handler
            var handler = Interlocked.Exchange<EventHandler>(ref m_NegotiateCompleted, null);

            if (handler == null)
                return;

            handler(this, EventArgs.Empty);
        }
    }
}
