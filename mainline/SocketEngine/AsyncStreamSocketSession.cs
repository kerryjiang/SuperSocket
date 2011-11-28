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

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private byte[] m_ReadBuffer;

        public AsyncStreamSocketSession(Socket client, ICommandReader<TCommandInfo> initialCommandReader)
            : base(client, initialCommandReader)
        {
            
        }

        /// <summary>
        /// Starts this session communication.
        /// </summary>
        public override void Start()
        {
            //Hasn't started, but already closed
            if (IsClosed)
                return;

            m_ReadBuffer = new byte[Client.ReceiveBufferSize];

            try
            {
                SecureProtocol = AppServer.BasicSecurity;

                var asyncResult = BeginInitStream(OnBeginInitStreamOnSessionStarted);

                //If the operation is synchronous
                if (asyncResult == null)
                    OnSessionStarting();
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                Close(CloseReason.SocketError);
                return;
            }
        }

        private void OnSessionStarting()
        {
            m_Stream.BeginRead(m_ReadBuffer, 0, m_ReadBuffer.Length, OnStreamEndRead, m_Stream);
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
                    AppSession.Status = SessionStatus.Healthy;

                    int thisLeft, thisLength;
                    thisLength = thisRead;
                    thisLeft = thisRead;

                    while (thisLeft > 0)
                    {
                        TCommandInfo commandInfo = FindCommand(m_ReadBuffer, thisRead - thisLength, thisLength, true, out thisLeft);
                        thisLength = thisLeft;

                        if (commandInfo != null)
                        {
                            ExecuteCommand(commandInfo);

                            if (Client == null && !IsClosed)
                            {
                                Close(CloseReason.ServerClosing);
                                return;
                            }
                        }
                    }

                    m_Stream.BeginRead(m_ReadBuffer, 0, m_ReadBuffer.Length, OnStreamEndRead, m_Stream);
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

                AppServer.Logger.LogError(this, ioe);
                this.Close(CloseReason.SocketError);
                return;
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(this, e);
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
                    result = sslStream.BeginAuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Default, false, asyncCallback, sslStream);
                    break;
                case (SslProtocols.Ssl2):
                    SslStream ssl2Stream = new SslStream(new NetworkStream(Client), false);
                    result = ssl2Stream.BeginAuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Ssl2, false, asyncCallback, ssl2Stream);
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
                AppSession.Logger.LogError(e);
                this.Close(CloseReason.SocketError);
                return;
            }

            m_Stream = sslStream;
        }

        public override void SendResponse(string message)
        {
            byte[] data = AppSession.Charset.GetBytes(message);
            SendResponse(data, 0, data.Length);
        }

        public override void SendResponse(byte[] data)
        {
            SendResponse(data, 0, data.Length);
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
    }
}
