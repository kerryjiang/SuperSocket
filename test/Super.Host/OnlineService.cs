using Microsoft.Extensions.Options;
using Super.Engine;
using SuperSocket;
using SuperSocket.Server;
using System;

namespace Super.Host
{
    /// <summary>
    /// 
    /// </summary>
    public class OnlineService : SuperSocketService<OnlinePackageInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serverOptions"></param>
        public OnlineService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions)
            : base(serviceProvider, serverOptions)
        {

        }
    }
}
