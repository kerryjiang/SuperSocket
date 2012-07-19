using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, ICommandSource<ICommand<TAppSession, TRequestInfo>>, IRawDataProcessor<TAppSession>, IRequestHandler<TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Setups the specified root config, this method used for programming setup
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="requestFilterFactory">The request filter factory.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="connectionFilters">The connection filters.</param>
        /// <param name="commandLoaders">The command loaders.</param>
        /// <returns></returns>
        public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<TRequestInfo> requestFilterFactory = null, ILogFactory logFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null, IEnumerable<ICommandLoader> commandLoaders = null)
        {
            SetupBasic(rootConfig, config, socketServerFactory);

            if (!SetupLogFactory(logFactory))
                return false;

            Logger = CreateLogger(this.Name);

            if (!SetupMedium(requestFilterFactory, connectionFilters, commandLoaders))
                return false;

            if (!SetupAdvanced(config))
                return false;

            if (!Setup(rootConfig, config))
                return false;

            return SetupFinal();
        }
    }
}
