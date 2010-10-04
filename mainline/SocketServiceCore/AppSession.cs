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
        IPEndPoint RemoteEndPoint { get; }
        void Close();
        void SayWelcome();
        void HandleUnknownCommand(CommandInfo cmdInfo);
        void HandleExceptionalError(Exception e);
    }

    public interface IAppSession<T>
        where T : IAppSession, new()
    {
        void Initialize(IAppServer<T> server, ISocketSession socketSession);
    }

    public abstract class AppSession<TAppSession, TAppServer, TSocketContext> : IAppSession, IAppSession<TAppSession>
        where TAppServer : AppServer<TAppSession>
        where TAppSession : IAppSession, IAppSession<TAppSession>, new()
        where TSocketContext : SocketContext, new()
    {
        public TAppServer AppServer { get; private set; }

        public AppSession()
        {
            SessionID = Guid.NewGuid().ToString(); 
        }

        public virtual void Initialize(IAppServer<TAppSession> appServer, ISocketSession socketSession)
        {
            AppContext = new TSocketContext();
            AppServer = appServer as TAppServer;
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

        protected virtual void OnInit()
        {

        }

        public abstract void SayWelcome();

        public abstract void HandleExceptionalError(Exception e);

        public virtual void HandleUnknownCommand(CommandInfo cmdInfo)
        {
            SendResponse("Unknown command: " + cmdInfo.Name);
        }

        public SocketContext Context
        {
            get { return AppContext; }
        }

        public TSocketContext AppContext { get; private set; }

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

    public abstract class AppSession<TAppSession, TAppServer> : AppSession<TAppSession, TAppServer, SocketContext>
        where TAppServer : AppServer<TAppSession>
        where TAppSession : IAppSession, IAppSession<TAppSession>, new()
    {

    }

    public abstract class AppSession<TAppSession> : AppSession<TAppSession, AppServer<TAppSession>, SocketContext>
        where TAppSession : IAppSession, IAppSession<TAppSession>, new()
    {

    }
}
