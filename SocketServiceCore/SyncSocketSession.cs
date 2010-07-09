using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using System.IO;
using System.Security.Authentication;
using System.Net.Security;

namespace SuperSocket.SocketServiceCore
{
    public class SyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
    {
        /// <summary>
        /// Starts the the session with specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Start(SocketContext context)
        {
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            InitStream(context);

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
                        //Has been closed
                        OnClose();
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
                    LogUtil.LogError(e);
                    HandleExceptionalError(e);
                }
            }

            if (Client != null)
            {
                Close();
            }
            else if (!IsClosed)
            {
                OnClose();
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
                    sslStream.AuthenticateAsServer(AuthenticationManager.GetCertificate(), false, SslProtocols.Default, true);
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
            catch (Exception e)
            {
                LogUtil.LogError(e);
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
            if (string.IsNullOrEmpty(message))
                return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            byte[] data = context.Charset.GetBytes(message);

            try
            {
                m_Stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                this.Close();
            }
        }
    }
}
