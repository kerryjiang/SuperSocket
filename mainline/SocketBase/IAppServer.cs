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
using System.Collections;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase
{
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
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItem
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Setups with the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories);


        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();


        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }
    }

    /// <summary>
    /// The interface for AppServer
    /// </summary>
    public interface IAppServer : IWorkItem, ILoggerProvider
    {
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
        /// Gets the request filter factory.
        /// </summary>
        object RequestFilterFactory { get; }
        

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

        /// <summary>
        /// Gets the log factory.
        /// </summary>
        ILogFactory LogFactory { get; }
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

    /// <summary>
    /// The interface for handler of session request
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IRequestHandler<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void ExecuteCommand(IAppSession session, TRequestInfo requestInfo);
    }
}
