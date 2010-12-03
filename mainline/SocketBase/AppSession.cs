using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    public abstract class AppSession<TAppSession, TCommandInfo> : IAppSession, IAppSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public virtual IAppServer<TAppSession, TCommandInfo> AppServer { get; private set; }

        protected virtual SocketContext CreateSocketContext()
        {
            return new SocketContext();
        }

        public virtual void Initialize(IAppServer<TAppSession, TCommandInfo> appServer, ISocketSession socketSession)
        {
            Context = CreateSocketContext();
            AppServer = appServer;
            SocketSession = socketSession;
            SocketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(SocketSession_Closed);
            SessionID = socketSession.SessionID;
            IdentityKey = socketSession.IdentityKey;
            OnInit();
        }

        void SocketSession_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            OnClosed();
        }

        protected abstract void OnClosed();

        protected virtual void OnInit()
        {
            this.StartTime = DateTime.Now;
        }

        public virtual void StartSession()
        {

        }

        public abstract void HandleExceptionalError(Exception e);

        public void ExecuteCommand(TAppSession session, TCommandInfo cmdInfo)
        {
            AppServer.ExecuteCommand(session, cmdInfo);
        }

        public virtual void HandleUnknownCommand(TCommandInfo cmdInfo)
        {
            SendResponse("Unknown command: " + cmdInfo.CommandKey);
        }

        public virtual SocketContext Context { get; private set; }

        public SslProtocols SecureProtocol
        {
            get { return SocketSession.SecureProtocol; }
            set { SocketSession.SecureProtocol = value; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; private set; }

        public string SessionID { get; private set; }

        public string IdentityKey { get; private set; }

        public ISocketSession SocketSession { get; private set; }

        public IServerConfig Config
        {
            get { return AppServer.Config; }
        }

        public virtual void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
            OnClosed();
        }

        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        public virtual void SendResponse(string message)
        {
            SocketSession.SendResponse(Context, message);
        }

        public virtual void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(Context, string.Format(message, paramValues));
        }

        public virtual void SendResponse(byte[] data)
        {
            SocketSession.SendResponse(Context, data);
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
}
