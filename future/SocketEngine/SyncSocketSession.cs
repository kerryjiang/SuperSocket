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
        
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            m_ReadBuffer = new byte[Client.ReceiveBufferSize];

            try
            {
                SecureProtocol = AppServer.BasicSecurity;
                InitStream(AppSession.Context);
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
                AppSession.Context.Status = SocketContextStatus.Healthy;

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

        private void InitStream(SocketContext context)
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

        public override void ApplySecureProtocol(SocketContext context)
        {
            InitStream(context);
        }

        private bool TryGetCommand(out TCommandInfo commandInfo)
        {
            commandInfo = NullCommandInfo;

            try
            {
                int thisRead = 0;

                while (true)
                {
                    thisRead = m_Stream.Read(m_ReadBuffer, 0, m_ReadBuffer.Length);
                    if (thisRead <= 0)
                    {
                        this.Close(CloseReason.ClientClosing);
                        return false;
                    }

                    commandInfo = FindCommand(m_ReadBuffer, 0, thisRead, true);

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

        public override void SendResponse(SocketContext context, string message)
        {
            byte[] data = context.Charset.GetBytes(message);

            try
            {
                m_Stream.Write(data, 0, data.Length);
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

        public override void SendResponse(SocketContext context, byte[] data)
        {
            if (data == null || data.Length <= 0)
                return;

            try
            {
                m_Stream.Write(data, 0, data.Length);
                m_Stream.Flush();
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(this, e);
                this.Close(CloseReason.SocketError);
            }
        }

        public override void ReceiveData(Stream storeSteram, int length)
        {
            byte[] buffer = new byte[Client.ReceiveBufferSize];

            int thisRead = 0;
            int leftRead = length;
            int shouldRead = 0;

            while (leftRead > 0)
            {
                shouldRead = Math.Min(buffer.Length, leftRead);
                thisRead = m_Stream.Read(buffer, 0, shouldRead);
                this.AppSession.LastActiveTime = DateTime.Now;

                if (thisRead <= 0)
                {
                    //Slow speed? Wait a moment
                    Thread.Sleep(100);
                    continue;
                }

                storeSteram.Write(buffer, 0, thisRead);
                leftRead -= thisRead;
            }
        }

        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            byte[] buffer = new byte[Client.ReceiveBufferSize];
            byte[] lastData = new byte[endMark.Length];
            int lastDataSzie = 0;

            int thisRead = 0;

            while (true)
            {
                thisRead = m_Stream.Read(buffer, 0, buffer.Length);
                this.AppSession.LastActiveTime = DateTime.Now;

                if (thisRead <= 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                storeSteram.Write(buffer, 0, thisRead);

                if (DetectEndMark(buffer, thisRead, endMark, lastData, ref lastDataSzie))
                    return;
            }
        }
    }
}
