using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The basic interface for appSession
    /// </summary>
    public interface IAppSession : ISessionBase
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        IAppServer AppServer { get; }
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
        /// Handles the exceptional error.
        /// </summary>
        /// <param name="e">The e.</param>
        void HandleException(Exception e);

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAppSession"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool Connected { get; }

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
        ILog Logger { get; }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns>return offset delta of next receiving buffer</returns>
        int ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied);

        /// <summary>
        /// Starts the session.
        /// </summary>
        void StartSession();


        /// <summary>
        /// Tries to get the data segment to be sent.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>
        /// return whether has data to send
        /// </returns>
        bool TryGetSendingData(IList<ArraySegment<byte>> segments);
    }

    /// <summary>
    /// The interface for appSession
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppSession<TRequestInfo> : IAppSession
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Handles the unknown request.
        /// </summary>
        /// <param name="requestInfo">The request info.</param>
        void HandleUnknownRequest(TRequestInfo requestInfo);
    }

    /// <summary>
    /// The interface for appSession
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppSession<TAppSession, TRequestInfo> : IAppSession<TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        /// <summary>
        /// Initializes the specified session.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <param name="requestFilter">The request filter.</param>
        void Initialize(IAppServer<TAppSession, TRequestInfo> server, ISocketSession socketSession, IRequestFilter<TRequestInfo> requestFilter);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cmdInfo">The request info.</param>
        void ExecuteCommand(TAppSession session, TRequestInfo cmdInfo);
    }
}
