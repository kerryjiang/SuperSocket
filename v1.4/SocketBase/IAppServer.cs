using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    public interface ILoggerProvider
    {
        ILogger Logger { get; }
    }

    public interface IPerformanceDataSource
    {
        /// <summary>
        /// Collects the performance data.
        /// </summary>
        /// <param name="globalPerfData">The global perf data.</param>
        /// <returns></returns>
        PerformanceData CollectPerformanceData(GlobalPerformanceData globalPerfData);
    }

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
        /// <param name="rootConfig">The SuperSocket root config.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <returns></returns>
        bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory);

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the server's config.
        /// </summary>
        IServerConfig Config { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the started time.
        /// </summary>
        DateTime StartedTime { get; }
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Gets the certificate of current server.
        /// </summary>
        X509Certificate Certificate { get; }

        /// <summary>
        /// Gets the transfer layer security protocol.
        /// </summary>
        SslProtocols BasicSecurity { get; }

        /// <summary>
        /// Creates a new app session by a socket session.
        /// </summary>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        TAppSession CreateAppSession(ISocketSession socketSession);

        /// <summary>
        /// Gets the app session by indentity key from server's session container.
        /// </summary>
        /// <param name="identityKey">The identity key.</param>
        /// <returns></returns>
        TAppSession GetAppSessionByIndentityKey(string identityKey);

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
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }
    }

    public interface IAppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession>, ICommandSource<ICommand<TAppSession, TCommandInfo>>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TCommandInfo>
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandInfo">The command info.</param>
        void ExecuteCommand(TAppSession session, TCommandInfo commandInfo);
    }
}
