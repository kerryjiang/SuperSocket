using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
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
        private ICommandStreamReader<TCommandInfo> m_CommandReader;

        public SyncSocketSession(ICommandStreamReader<TCommandInfo> commandReader)
        {
            m_CommandReader = commandReader;
        }

        /// <summary>
        /// Starts the the session with specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Start(SocketContext context)
        {
            //Hasn't started, but already closed
            if (IsClosed)
                return;
        
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            try
            {
                SecureProtocol = Config.Security;
                InitStream(context);
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                Close(CloseReason.SocketError);
                return;
            }

            StartSession();

            TCommandInfo commandInfo;

            while (TryGetCommand(out commandInfo))
            {
                context.Status = SocketContextStatus.Healthy;

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
                    LogUtil.LogError(AppServer, e);
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
                    sslStream.AuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Default, true);
                    break;
                case (SslProtocols.Ssl2):
                    SslStream ssl2Stream = new SslStream(new NetworkStream(Client), false);
                    ssl2Stream.AuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Ssl2, true);
                    m_Stream = ssl2Stream as Stream;
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }

            if (context == null)
                m_CommandReader.InitializeReader(context, m_Stream, Encoding.Default, 0x400);
            else
                m_CommandReader.InitializeReader(context, m_Stream, context.Charset, 0x400);
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            InitStream(context);
        }

        private bool TryGetCommand(out TCommandInfo commandInfo)
        {
            commandInfo = default(TCommandInfo);

            try
            {
                commandInfo = m_CommandReader.ReadCommand();
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

                LogUtil.LogError(AppServer, "An error occurred in session: " + this.SessionID, ioe);
                this.Close(CloseReason.SocketError);
                return false;
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
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

                LogUtil.LogError(AppServer, e.GetType().ToString());
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
                LogUtil.LogError(AppServer, e);
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
