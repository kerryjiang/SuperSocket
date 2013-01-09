using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    public abstract partial class AppServerBase<TAppSession, TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        partial void SetDefaultCulture(IRootConfig rootConfig, IServerConfig config)
        {
            var defaultCulture = config.DefaultCulture;
            
            //default culture has been set for this server instance
            if (!string.IsNullOrEmpty(defaultCulture))
            {
                if (rootConfig.Isolation == IsolationMode.None)
                {
                    Logger.WarnFormat("The default culture '{0}' cannot be set, because you cannot set default culture for one server instance if the Isolation is None!");
                    return;
                }
            }
            else if(!string.IsNullOrEmpty(rootConfig.DefaultCulture))
            {
                defaultCulture = rootConfig.DefaultCulture;

                //Needn't set default culture in this case, because it has been set in the bootstrap
                if (rootConfig.Isolation == IsolationMode.None)
                    return;
            }

            if (string.IsNullOrEmpty(defaultCulture))
                return;

            try
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(defaultCulture);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Failed to set default culture '{0}'.", defaultCulture), e);
            }
        }
    }
}
