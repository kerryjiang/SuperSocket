using System;
using Microsoft.Extensions.Options;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Server
{
    interface IWebSocketCommandMiddleware : IMiddleware
    {

    }

    public class WebSocketCommandMiddleware<TKey, TPackageInfo> : CommandMiddleware<TKey, WebSocketPackage, TPackageInfo>, IWebSocketCommandMiddleware
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
            
        }

        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(serviceProvider, commandOptions, mapper)
        {

        }
    }
}