using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public interface IAppSession : ISessionBase
    {
        /// <summary>
        /// Gets the socket session of the AppSession.
        /// </summary>
        ISocketSession SocketSession { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        IDictionary<object, object> Items { get; }

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

        /// <summary>
        /// Gets or sets the status of session.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        SessionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the charset which is used for transfering text message.
        /// </summary>
        /// <value>The charset.</value>
        Encoding Charset { get; set; }

        /// <summary>
        /// Gets or sets the previous command.
        /// </summary>
        /// <value>
        /// The prev command.
        /// </value>
        string PrevCommand { get; set; }

        /// <summary>
        /// Gets or sets the current executing command.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        string CurrentCommand { get; set; }

        /// <summary>
        /// Gets the logger assosiated with this session.
        /// </summary>
        ILogger Logger { get; }
    }

    public interface IAppSession<TCommandInfo> : IAppSession
        where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Handles the unknown command.
        /// </summary>
        /// <param name="cmdInfo">The command info.</param>
        void HandleUnknownCommand(TCommandInfo cmdInfo);
        
        /// <summary>
        /// Gets or sets the next command reader for next round receiving.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        ICommandReader<TCommandInfo> NextCommandReader { get; set; }
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
