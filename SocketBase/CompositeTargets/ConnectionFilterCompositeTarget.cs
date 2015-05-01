using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase.CompositeTargets
{
    class ConnectionFilterCompositeTarget : MultipleResultCompositeTarget<IConnectionFilter>
    {
        public ConnectionFilterCompositeTarget(Action<List<IConnectionFilter>> callback)
            : base((config) => config.ConnectionFilter, callback, false)
        {

        }
        protected override bool PrepareResult(IConnectionFilter result, IAppServer appServer, IProviderMetadata metadata)
        {
            if (!result.Initialize(metadata.Name, appServer))
            {
                appServer.Logger.ErrorFormat("Failed to initialize the ConnectionFilter '{0}'", metadata.Name);
                return false;
            }

            return true;
        }
    }
}
