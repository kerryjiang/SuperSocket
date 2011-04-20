using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    public interface IAppSession : ISessionBase
    {
        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        ISocketSession SocketSession { get; }

        /// <summary>
        /// Gets the context of the socket session.
        /// </summary>
        SocketContext Context { get; }

        /// <summary>
        /// Gets the config of the server.
        /// </summary>
        IServerConfig Config { get; }

        /// <summary>
        /// Gets the local listening endpoint.
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets or sets the last active time of the session.
        /// </summary>
        /// <value>
        /// The last active time.
        /// </value>
        DateTime LastActiveTime { get; set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Closes this session.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes the session by the specified reason.
        /// </summary>
        /// <param name="reason">The close reason.</param>
        void Close(CloseReason reason);

        /// <summary>
        /// Starts the session.
        /// </summary>
        void StartSession();

        /// <summary>
        /// Handles the exceptional error.
        /// </summary>
        /// <param name="e">The e.</param>
        void HandleExceptionalError(Exception e);
    }

    public interface IAppSession<TCommandInfo> : IAppSession
        where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Handles the unknown command.
        /// </summary>
        /// <param name="cmdInfo">The command info.</param>
        void HandleUnknownCommand(TCommandInfo cmdInfo);
    }

    public interface IAppSession<TAppSession, TCommandInfo> : IAppSession<TCommandInfo>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession, IAppSession<TCommandInfo>, new()
    {
        /// <summary>
        /// Initializes the specified session.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="socketSession">The socket session.</param>
        void Initialize(IAppServer<TAppSession, TCommandInfo> server, ISocketSession socketSession);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cmdInfo">The command info.</param>
        void ExecuteCommand(TAppSession session, TCommandInfo cmdInfo);
    }

    public class AppSessionClosedEventArgs<TAppSession> : EventArgs
        where TAppSession : IAppSession, new()
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        public TAppSession Session { get; private set; }

        /// <summary>
        /// Gets the close reason.
        /// </summary>
        public CloseReason Reason { get; private set; }

        public AppSessionClosedEventArgs(TAppSession session, CloseReason reason)
        {
            Session = session;
            Reason = reason;
        }
    }
}
