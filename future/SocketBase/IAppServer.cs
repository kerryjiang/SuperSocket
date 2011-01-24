using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.SocketBase.Command;
using System.ServiceModel.Description;
using SuperSocket.Common;
using System.Security.Authentication;

namespace SuperSocket.SocketBase
{
    public interface ILoggerProvider
    {
        ILogger Logger { get; }
    }

    public interface IAppServer : ILoggerProvider
    {
        /// <summary>
        /// Gets the name of the server instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the server credentials for client console
        /// </summary>
        /// <value>
        /// The server credentials.
        /// </value>
        ServiceCredentials ServerCredentials { get; set; }

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="rootConfig">The SuperSocket root config.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="assembly">The service provider assembly.</param>
        /// <returns></returns>
        bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, string providerAssembly);

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
        /// Logs the current performance.
        /// </summary>
        void LogPerf();
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        X509Certificate Certificate { get; }
        SslProtocols BasicSecurity { get; }
        TAppSession CreateAppSession(ISocketSession socketSession);
        TAppSession GetAppSessionByIndentityKey(string identityKey);
        int SessionCount { get; }
    }

    public interface IAppServer<TAppSession, TCommandInfo> : IAppServer<TAppSession>, ICommandSource<ICommand<TAppSession, TCommandInfo>>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession<TCommandInfo>
    {
        void ExecuteCommand(TAppSession session, TCommandInfo commandInfo);
    }
}
