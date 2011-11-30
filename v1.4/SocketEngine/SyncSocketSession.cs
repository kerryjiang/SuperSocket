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
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;

namespace SuperSocket.SocketEngine
{
    class SyncSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private byte[] m_ReadBuffer;
        private int m_ReadBufferLeft = 0;
        private int m_ReadBufferTotal = 0;

        public SyncSocketSession(Socket client, ICommandReader<TCommandInfo> commandReader)
            : base(client, commandReader)
        {

        }

        /// <summary>
        /// Starts the the session.
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
                InitStream();
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                Close(CloseReason.SocketError);
                return;
            }

            StartSession();

            TCommandInfo commandInfo;

            while (TryGetCommand(out commandInfo))
            {
                AppSession.Status = SessionStatus.Healthy;

                try
                {
                    ExecuteCommand(commandInfo);

                    if (Client == null && !IsClosed)
                    {
                        Close(CloseReason.ServerClosing);
                        return;
                    }
                }
                catch (SocketException)
                {
                    Close(CloseReason.SocketError);
                    break;
                }
                catch (Exception e)
                {
                    AppServer.Logger.LogError(this, e);
                    HandleExceptionalError(e);
                }
            }

            if (Client != null && !IsClosed)
            {
                Close(CloseReason.ServerClosing);
            }
        }


        public override void Close(CloseReason reason)
        {
            base.Close(reason);
        }

        private Stream m_Stream;

        private void InitStream()
        {
            switch (SecureProtocol)
            {
                case (SslProtocols.Default):
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                    SslStream sslStream = new SslStream(new NetworkStream(Client), false);
                    sslStream.AuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Default, false);
                    m_Stream = sslStream;
                    break;
                case (SslProtocols.Ssl2):
                    SslStream ssl2Stream = new SslStream(new NetworkStream(Client), false);
                    ssl2Stream.AuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Ssl2, false);
                    m_Stream = ssl2Stream;
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }
        }

        public override void ApplySecureProtocol()
        {
            InitStream();
        }

        private bool TryGetCommand(out TCommandInfo commandInfo)
        {
            commandInfo = NullCommandInfo;

            try
            {
                if (m_ReadBufferLeft > 0)
                {
                    int left;
                    commandInfo = FindCommand(m_ReadBuffer, m_ReadBufferTotal - m_ReadBufferLeft, m_ReadBufferLeft, true, out left);

                    m_ReadBufferLeft = left;

                    if (commandInfo != null)
                        return true;
                }

                int thisRead = 0;

                while (true)
                {
                    thisRead = m_Stream.Read(m_ReadBuffer, 0, m_ReadBuffer.Length);
                    if (thisRead <= 0)
                    {
                        this.Close(CloseReason.ClientClosing);
                        return false;
                    }

                    commandInfo = FindCommand(m_ReadBuffer, 0, thisRead, true, out m_ReadBufferLeft);

                    if (m_ReadBufferLeft > 0)
                    {
                        m_ReadBufferTotal = thisRead;
                    }

                    if (commandInfo != null || IsClosed)
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                this.Close(CloseReason.SocketError);
                return false;
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
                            return false;
                        }
                    }

                    if (ioe.InnerException is ObjectDisposedException)
                    {
                        this.Close(CloseReason.SocketError);
                        return false;
                    }
                }

                AppServer.Logger.LogError(this, ioe);
                this.Close(CloseReason.SocketError);
                return false;
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(this, e);
                this.Close(CloseReason.Unknown);
                return false;
            }

            return commandInfo != null;
        }

        public override void SendResponse(string message)
        {
            byte[] data = AppSession.Charset.GetBytes(message);

            try
            {
                m_Stream.Write(data, 0, data.Length);
                m_Stream.Flush();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    SocketException se = e.InnerException as SocketException;
                    if (se != null)
                    {
                        if (se.ErrorCode == 10054)
                        {
                            this.Close(CloseReason.SocketError);
                            return;
                        }
                        else if (se.ErrorCode == 10053)
                        {
                            this.Close(CloseReason.ServerClosing);
                            return;
                        }
                    }
                }

                AppServer.Logger.LogError(this, e);
                this.Close(CloseReason.Unknown);
            }
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
            catch (Exception e)
            {
                AppServer.Logger.LogError(this, e);
                this.Close(CloseReason.SocketError);
            }
        }
    }
}
