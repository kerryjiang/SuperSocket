using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public abstract class AppSession<TAppSession, TCommandInfo> : IAppSession, IAppSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        #region Attributes

        /// <summary>
        /// Gets the app server.
        /// </summary>
        public virtual IAppServer<TAppSession, TCommandInfo> AppServer { get; private set; }

        /// <summary>
        /// Gets or sets the charset which is used for transfering text message.
        /// </summary>
        /// <value>
        /// The charset.
        /// </value>
        public Encoding Charset { get; set; }

        private IDictionary<object, object> m_Items;

        /// <summary>
        /// Gets the items dictionary, only support 10 items maximum
        /// </summary>
        public IDictionary<object, object> Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new Dictionary<object, object>(10);

                return m_Items;
            }
        }

        /// <summary>
        /// Gets or sets the status of session.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SessionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the previous command.
        /// </summary>
        /// <value>
        /// The prev command.
        /// </value>
        public string PrevCommand { get; set; }

        /// <summary>
        /// Gets or sets the current executing command.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        public string CurrentCommand { get; set; }


        /// <summary>
        /// Gets or sets the secure protocol of transportation layer.
        /// </summary>
        /// <value>
        /// The secure protocol.
        /// </value>
        public SslProtocols SecureProtocol
        {
            get { return SocketSession.SecureProtocol; }
            set { SocketSession.SecureProtocol = value; }
        }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the remote endpoint of client.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger
        {
            get { return AppServer.Logger; }
        }

        /// <summary>
        /// Gets or sets the last active time of the session.
        /// </summary>
        /// <value>
        /// The last active time.
        /// </value>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }

        /// <summary>
        /// Gets the identity key.
        /// In most case, IdentityKey is same as SessionID
        /// </summary>
        public string IdentityKey { get; private set; }

        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        public ISocketSession SocketSession { get; private set; }

        /// <summary>
        /// Gets the config of the server.
        /// </summary>
        public IServerConfig Config
        {
            get { return AppServer.Config; }
        }
        
        /// <summary>
        /// Gets or sets the next command reader.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        ICommandReader<TCommandInfo> IAppSession<TCommandInfo>.NextCommandReader { get; set; }

        #endregion

        public AppSession()
        {
            this.StartTime = DateTime.Now;
            this.LastActiveTime = this.StartTime;
            this.Charset = Encoding.UTF8;
        }

        /// <summary>
        /// Initializes the specified app session by AppServer and SocketSession.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        public virtual void Initialize(IAppServer<TAppSession, TCommandInfo> appServer, ISocketSession socketSession)
        {
            AppServer = appServer;
            SocketSession = socketSession;
            SessionID = socketSession.SessionID;
            IdentityKey = socketSession.IdentityKey;
            Status = SessionStatus.Healthy;
            OnInit();
        }

        /// <summary>
        /// Called when [init].
        /// </summary>
        protected virtual void OnInit()
        {
            
        }

        /// <summary>
        /// Starts the session.
        /// </summary>
        public virtual void StartSession()
        {

        }

        /// <summary>
        /// Handles the exceptional error.
        /// </summary>
        /// <param name="e">The e.</param>
        public virtual void HandleExceptionalError(Exception e)
        {
            Logger.LogError(e);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cmdInfo">The CMD info.</param>
        public void ExecuteCommand(TAppSession session, TCommandInfo cmdInfo)
        {
            AppServer.ExecuteCommand(session, cmdInfo);
        }

        /// <summary>
        /// Handles the unknown command.
        /// </summary>
        /// <param name="cmdInfo">The CMD info.</param>
        public virtual void HandleUnknownCommand(TCommandInfo cmdInfo)
        {
            SendResponse("Unknown command: " + cmdInfo.Key);
        }

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        public virtual void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
            Status = SessionStatus.Disconnected;
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        public virtual void SendResponse(string message)
        {
            SocketSession.SendResponse(message);
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="message">The message which will be sent.</param>
        /// <param name="paramValues">The parameter values.</param>
        public virtual void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(string.Format(message, paramValues));
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="data">The data which will be sent.</param>
        public virtual void SendResponse(byte[] data)
        {
            SocketSession.SendResponse(data);
        }
        
        /// <summary>
        /// Sets the next command reader for next round receiving.
        /// </summary>
        /// <param name="nextCommandReader">The next command reader.</param>
        public void SetNextCommandReader(ICommandReader<TCommandInfo> nextCommandReader)
        {
            ((IAppSession<TCommandInfo>)this).NextCommandReader = nextCommandReader;
        }
    }

    public abstract class AppSession<TAppSession> : AppSession<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {

        private bool m_AppendNewLineForResponse = false;

        public AppSession()
            : this(true)
        {

        }

        public AppSession(bool appendNewLineForResponse)
        {
            m_AppendNewLineForResponse = appendNewLineForResponse;
        }

        protected virtual string ProcessSendingMessage(string rawMessage)
        {
            if (!m_AppendNewLineForResponse)
                return rawMessage;

            if (AppServer.Config.Mode == SocketMode.Udp)
                return rawMessage;

            if (string.IsNullOrEmpty(rawMessage) || !rawMessage.EndsWith(Environment.NewLine))
                return rawMessage + Environment.NewLine;
            else
                return rawMessage;
        }

        public override void SendResponse(string message)
        {
            base.SendResponse(ProcessSendingMessage(message));
        }

        public override void SendResponse(string message, params object[] paramValues)
        {
            base.SendResponse(ProcessSendingMessage(message), paramValues);
        }
    }

    public class AppSession : AppSession<AppSession>
    {

    }
}
