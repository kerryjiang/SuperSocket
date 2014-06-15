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
using SuperSocket.ProtoBase;

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
        /// Creates the pipeline processor.
        /// </summary>
        /// <returns></returns>
        IPipelineProcessor CreatePipelineProcessor();

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
        /// Gets the logger assosiated with this session.
        /// </summary>
        ILog Logger { get; }

        /// <summary>
        /// Starts the session.
        /// </summary>
        void StartSession();
    }

    /// <summary>
    /// The interface for appSession
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
    public interface IAppSession<TAppSession, TPackageInfo> : IAppSession
        where TPackageInfo : IPackageInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TPackageInfo>, new()
    {
        /// <summary>
        /// Initializes the specified session.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="socketSession">The socket session.</param>
        void Initialize(IAppServer<TAppSession, TPackageInfo> server, ISocketSession socketSession);
    }
}
