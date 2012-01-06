using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract class SocketSession<TAppSession, TRequestInfo> : ISocketSession<TAppSession>
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        public IAppServer<TAppSession> AppServer { get; private set; }

        protected ICommandReader<TRequestInfo> CommandReader { get; private set; }

        private static readonly TRequestInfo m_NullRequestInfo = default(TRequestInfo);

        protected TRequestInfo NullRequestInfo
        {
            get { return m_NullRequestInfo; }
        }

        public TAppSession AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        public SocketSession(Socket client, ICommandReader<TRequestInfo> commandReader)
            : this(Guid.NewGuid().ToString(), commandReader)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            Client = client;
            LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public SocketSession(string sessionID, ICommandReader<TRequestInfo> commandReader)
        {
            SessionID = sessionID;
            CommandReader = commandReader;
        }

        public virtual void Initialize(IAppServer<TAppSession> appServer, TAppSession appSession)
        {
            AppServer = appServer;
            AppSession = appSession;
        }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID { get; private set; }


        public IServerConfig Config { get; set; }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public abstract void Start();

        protected virtual void ExecuteCommand(TRequestInfo requestInfo)
        {
            AppSession.ExecuteCommand(AppSession, requestInfo);

            if(AppSession.NextCommandReader != null)
            {
                CommandReader = AppSession.NextCommandReader;
                AppSession.NextCommandReader = null;
            }
        }

        protected internal TRequestInfo FindCommand(byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            var requestInfo = CommandReader.FindRequestInfo(AppSession, readBuffer, offset, length, isReusableBuffer, out left);

            if (requestInfo == null)
            {
                int leftBufferCount = CommandReader.LeftBufferSize;
                if (leftBufferCount >= AppServer.Config.MaxCommandLength)
                {
                    if (AppServer.Logger.IsErrorEnabled)
                        AppServer.Logger.ErrorFormat("Max command length: {0}, current processed length: {1}", AppServer.Config.MaxCommandLength, leftBufferCount);
                    Close(CloseReason.ServerClosing);
                    return m_NullRequestInfo;
                }
            }

            //If next command reader wasn't set, still use current command reader in next round received data processing
            if (CommandReader.NextCommandReader != null)
                CommandReader = CommandReader.NextCommandReader;

            return requestInfo;
        }

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void StartSession()
        {
            AppSession.LastActiveTime = DateTime.Now;
            AppSession.StartSession();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose(CloseReason reason)
        {
            var closedHandler = Closed;
            if (closedHandler != null)
            {
                closedHandler(this, new SocketSessionClosedEventArgs
                    {
                        SessionID = this.SessionID,
                        Reason = reason
                    });
            }
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public event EventHandler<SocketSessionClosedEventArgs> Closed;

        protected virtual void HandleExceptionalError(Exception e)
        {
            AppSession.HandleExceptionalError(e);
        }

        public abstract void SendResponse(string message);

        public abstract void SendResponse(byte[] data);
        
        public abstract void SendResponse(byte[] data, int offset, int length);

        public abstract void ApplySecureProtocol();

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
        public virtual IPEndPoint LocalEndPoint { get; protected set; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        public virtual void Close(CloseReason reason)
        {
            if (Client == null && m_IsClosed)
                return;

            lock (SyncRoot)
            {
                if (Client == null && m_IsClosed)
                    return;

                Client.SafeCloseClientSocket(AppServer.Logger);

                Client = null;
                m_IsClosed = true;
                OnClose(reason);
            }
        }
    }
}
