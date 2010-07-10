using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
    public interface IAppSession
    {
        ISocketSession SocketSession { get; }
        string SessionID { get; }
        SocketContext Context { get; }
        IServerConfig Config { get; }
        IPEndPoint LocalEndPoint { get; }
        void Close();
        void SayWelcome();
        void HandleExceptionalError(Exception e);
    }

    public interface IAppSession<T>
        where T : IAppSession, new()
    {
        void Initialize(IAppServer<T> server, ISocketSession socketSession);
        IAppServer<T> AppServer { get; }
    }

    public abstract class AppSession<T> : IAppSession, IAppSession<T>
        where T : IAppSession, new()
    {
        public IAppServer<T> AppServer { get; private set; }

        public AppSession()
        {
            SessionID = Guid.NewGuid().ToString(); 
        }

        public virtual void Initialize(IAppServer<T> appServer, ISocketSession socketSession)
        {
            AppServer = appServer;
            SocketSession = socketSession;
            SocketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(SocketSession_Closed);
            SessionID = socketSession.SessionID;
            OnInit();
        }

        void SocketSession_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            OnClosed();
        }

        protected abstract void OnClosed();

        protected abstract void OnInit();

        public abstract void SayWelcome();

        public abstract void HandleExceptionalError(Exception e);

        public abstract SocketContext Context { get; }

        public SslProtocols SecureProtocol
        {
            get { return SocketSession.SecureProtocol; }
            set { SocketSession.SecureProtocol = value; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        public string SessionID { get; private set; }

        public ISocketSession SocketSession { get; private set; }

        public IServerConfig Config
        {
            get { return AppServer.Config; }
        }

        public virtual void Close()
        {
            this.SocketSession.Close();
            OnClosed();
        }

        public void SendResponse(string message)
        {
            SocketSession.SendResponse(Context, message);
        }

        public void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(Context, string.Format(message, paramValues));
        }
    }    
}
