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
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Security;

namespace SuperSocket.SocketServiceCore
{
    class SyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
    {
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
                InitStream(context);
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                Close();
                return;
            }

            SayWelcome();

            string commandLine;

            while (TryGetCommand(out commandLine))
            {
                LastActiveTime = DateTime.Now;
                context.Status = SocketContextStatus.Healthy;

                try
                {
                    ExecuteCommand(commandLine);

                    if (Client == null && !IsClosed)
                    {
                        Close();
                        return;
                    }
                }
                catch (SocketException)
                {
                    Close();
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
                Close();
            }
        }


        public override void Close()
        {
            base.Close();
        }

        private Stream m_Stream;

        private void InitStream(SocketContext context)
        {
            switch (SecureProtocol)
            {
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                case (SslProtocols.Ssl2):
                    SslStream sslStream = new SslStream(new NetworkStream(Client), false);
                    sslStream.AuthenticateAsServer(AppServer.Certificate, false, SslProtocols.Default, true);
                    m_Stream = sslStream as Stream;
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }

            if (context == null)
                m_Reader = new StreamReader(m_Stream, Encoding.Default);
            else
                m_Reader = new StreamReader(m_Stream, context.Charset);
        }

        private StreamReader m_Reader = null;

        protected StreamReader SocketReader
        {
            get { return m_Reader; }
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            InitStream(context);
        }

        private bool TryGetCommand(out string command)
        {
            command = string.Empty;

            try
            {
                command = m_Reader.ReadLine();
            }
            catch (ObjectDisposedException)
            {
                this.Close();
                return false;
            }
            catch (IOException ioe)
            {
                if (ioe.InnerException != null)
                {
                    if (ioe.InnerException is SocketException)
                    {
                        var se = ioe.InnerException as SocketException;
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10053)
                        {
                            this.Close();
                            return false;
                        }
                    }

                    if (ioe.InnerException is ObjectDisposedException)
                    {
                        this.Close();
                        return false;
                    }
                }

                LogUtil.LogError(AppServer, "An error occurred in session: " + this.SessionID, ioe);
                this.Close();
                return false;
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                this.Close();
                return false;
            }

            if (string.IsNullOrEmpty(command))
                return false;

            command = command.Trim();

            if (string.IsNullOrEmpty(command))
                return false;

            return true;
        }

        public override void SendResponse(SocketContext context, string message)
        {
            if (string.IsNullOrEmpty(message) || !message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

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
                    if (se != null && se.ErrorCode == 10054)
                    {
                        this.Close();
                        return;
                    }
                }

                LogUtil.LogError(AppServer, e.GetType().ToString());
                this.Close();
            }
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            if (data == null || data.Length <= 0)
                return;

            try
            {
                m_Stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                LogUtil.LogError(AppServer, e);
                this.Close();
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
