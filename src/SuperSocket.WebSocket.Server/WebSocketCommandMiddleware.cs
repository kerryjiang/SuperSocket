using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket.Channel;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    interface IWebSocketCommandMiddleware : IMiddleware
    {

    }

    public class WebSocketCommandMiddleware<TKey, TPackageInfo, TPackageMapper> : CommandMiddleware<TKey, WebSocketPackage, TPackageInfo, TPackageMapper>, IWebSocketCommandMiddleware
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
        where TPackageMapper : IPackageMapper<WebSocketPackage, TPackageInfo>, new()
    {
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
            
        }
    }
}