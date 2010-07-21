using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using System.Security.Authentication;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    public abstract class SocketSession<T> : ISocketSession<T>
        where T : IAppSession, new()
    {
        public IAppServer<T> AppServer { get; private set; }
        public T AppSession { get; private set; }

        public SocketSession()
        {

        }

        public void Initialize(IAppServer<T> appServer, T appSession, Socket client)
        {
            AppServer = appServer;
            AppSession = appSession;
            Client = client;
        }

        /// <summary>
        /// The session identity string
        /// </summary>
        private string m_SessionID = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID
        {
            get { return m_SessionID; }
            set { m_SessionID = value; }
        }

        private DateTime m_StartTime = DateTime.Now;

        /// <summary>
        /// Gets the session start time.
        /// </summary>
        /// <value>The session start time.</value>
        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        private DateTime m_LastActiveTime = DateTime.Now;

        /// <summary>
        /// Gets the last active time of the session.
        /// </summary>
        /// <value>The last active time.</value>
        public DateTime LastActiveTime
        {
            get { return m_LastActiveTime; }
            protected set { m_LastActiveTime = value; }
        }

        private IServerConfig m_Config = null;

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>The config.</value>
        public IServerConfig Config
        {
            get { return m_Config; }
            set { m_Config = value; }
        }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public void Start()
        {
            Start(AppSession.Context);
        }

        protected abstract void Start(SocketContext context);


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cmdInfo">The CMD info.</param>
        protected virtual void ExecuteCommand(string commandLine)
        {
            CommandInfo cmdInfo = AppServer.CommandParser.ParseCommand(commandLine);
            ICommand<T> command = AppServer.GetCommandByName(cmdInfo.Name);

            if (command != null)
            {
                AppSession.Context.CurrentCommand = cmdInfo.Name;
                command.ExecuteCommand(AppSession, cmdInfo);
                AppSession.Context.PrevCommand = cmdInfo.Name;
                LastActiveTime = DateTime.Now;
                if (AppServer.Config.LogCommand)
                    LogUtil.LogInfo(AppServer, string.Format("Command - {0} - {1}", SessionID, cmdInfo.Name));
            }
        }

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void SayWelcome()
        {
            AppSession.SayWelcome();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose()
        {
            m_IsClosed = true;

            var closedHandler = Closed;
            if (closedHandler != null)
                closedHandler(this, new SocketSessionClosedEventArgs { SessionID = this.SessionID });
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public event EventHandler<SocketSessionClosedEventArgs> Closed;

        protected virtual void HandleExceptionalError(Exception e)
        {
            AppSession.HandleExceptionalError(e);
        }

        public abstract void SendResponse(SocketContext context, string message);

        public abstract void SendResponse(SocketContext context, byte[] data);

        public abstract void ApplySecureProtocol(SocketContext context);

        public abstract void ReceiveData(Stream storeSteram, int length);

        public abstract void ReceiveData(Stream storeSteram, byte[] endMark);

        public Stream GetUnderlyStream()
        {
            return new NetworkStream(Client);
        }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public Socket Client { get; set; }

        private bool m_IsClosed = false;

        protected bool IsClosed
        {
            get { return m_IsClosed; }
        }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>The local end point.</value>
        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)Client.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)Client.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        /// <summary>
        /// Closes this connection.
        /// </summary>
        public virtual void Close()
        {
            if (Client != null && !m_IsClosed)
            {
                try
                {
                    Client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, e);
                }

                try
                {
                    Client.Close();
                }
                catch (Exception e)
                {
                    LogUtil.LogError(AppServer, e);
                }
                finally
                {
                    Client = null;
                    m_IsClosed = true;
                    OnClose();
                }
            }
        }

        protected bool EndsWith(byte[] buffer, int offset, int length, byte[] endData)
        {
            if (length < endData.Length)
                return false;

            for (int i = 1; i <= endData.Length; i++)
            {
                if (endData[endData.Length - i] != buffer[offset + length - i])
                    return false;
            }

            return true;
        }
    }

    public class SocketSessionClosedEventArgs : EventArgs
    {
        public string SessionID { get; set; }
    }
}
