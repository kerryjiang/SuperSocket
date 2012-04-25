using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for who provides logger
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Gets the logger assosiated with this object.
        /// </summary>
        ILog Logger { get; }
    }

    /// <summary>
    /// The interface for who will react with performance collecting
    /// </summary>
    public interface IPerformanceDataSource
    {
        /// <summary>
        /// Collects the performance data.
        /// </summary>
        /// <param name="globalPerfData">The global perf data.</param>
        /// <returns></returns>
        PerformanceData CollectPerformanceData(GlobalPerformanceData globalPerfData);
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    public interface IAppServer : ILoggerProvider
    {
        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets or sets the server's connection filter
        /// </summary>
        /// <value>
        /// The server's connection filters
        /// </value>
        IEnumerable<IConnectionFilter> ConnectionFilters{ get; set; }

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="rootConfig">The SuperSocket root config.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <returns></returns>
        bool Setup(IBootstrap bootstrap, IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory);

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();

        /// <summary>
        /// Gets the started time.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        DateTime StartedTime { get; }


        /// <summary>
        /// Gets or sets the listeners.
        /// </summary>
        /// <value>
        /// The listeners.
        /// </value>
        ListenerInfo[] Listeners { get; }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the server's config.
        /// </summary>
        IServerConfig Config { get; }

        /// <summary>
        /// Gets the certificate of current server.
        /// </summary>
        X509Certificate Certificate { get; }

        /// <summary>
        /// Gets the transfer layer security protocol.
        /// </summary>
        SslProtocols BasicSecurity { get; }

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Creates the app session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        IAppSession CreateAppSession(ISocketSession socketSession);

        /// <summary>
        /// Gets the app session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        IAppSession GetAppSessionByID(string sessionID);

        /// <summary>
        /// Resets the session's security protocol.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="security">The security protocol.</param>
        void ResetSessionSecurity(IAppSession session, SslProtocols security);
    }

    /// <summary>
    /// The raw data processor
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface IRawDataProcessor<TAppSession>
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets or sets the raw binary data received event handler.
        /// TAppSession: session
        /// byte[]: receive buffer
        /// int: receive buffer offset
        /// int: receive lenght
        /// bool: whether process the received data further
        /// </summary>
        event Func<TAppSession, byte[], int, int, bool> RawDataReceived;
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the matched sessions from sessions snapshot.
        /// </summary>
        /// <param name="critera">The prediction critera.</param>
        /// <returns></returns>
        IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera);

        /// <summary>
        /// Gets all sessions in sessions snapshot.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TAppSession> GetAllSessions();

        /// <summary>
        /// Gets/sets the new session connected event handler.
        /// </summary>
        event Action<TAppSession> NewSessionConnected;

        /// <summary>
        /// Gets/sets the session closed event handler.
        /// </summary>
        event Action<TAppSession, CloseReason> SessionClosed;
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IAppServer<TAppSession, TRequestInfo> : IAppServer<TAppSession>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        /// <summary>
        /// Occurs when [request comming].
        /// </summary>
        event RequestHandler<TAppSession, TRequestInfo> RequestHandler;
    }
}
