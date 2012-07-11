using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common.Logging;

namespace SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, ICommandSource<ICommand<TAppSession, TRequestInfo>>, IRawDataProcessor<TAppSession>, IRequestHandler<TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        /// <summary>
        /// Setups the specified root config, used for programming setup
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="providers">The providers.</param>
        /// <returns></returns>
        public virtual bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, params object[] providers)
        {
            SetupBasic(rootConfig, config, socketServerFactory);

            if (!SetupLogFactory(GetProviderInstance<ILogFactory>(providers)))
                return false;

            Logger = CreateLogger(this.Name);

            if (!SetupMedium(GetProviderInstance<IRequestFilterFactory<TRequestInfo>>(providers), GetProviderInstance<IEnumerable<IConnectionFilter>>(providers), GetProviderInstance<IEnumerable<ICommandLoader>>(providers)))
                return false;

            if (!SetupAdvanced(config))
                return false;

            if (!Setup(rootConfig, config))
                return false;

            return SetupFinal();
        }

        private T GetProviderInstance<T>(object[] providers)
        {
            if(providers == null || !providers.Any())
                return default(T);

            var providerType = typeof(T);
            return (T)providers.FirstOrDefault(p => p != null && providerType.IsAssignableFrom(p.GetType()));
        }
    }
}
