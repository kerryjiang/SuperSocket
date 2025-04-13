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

    /// <summary>
    /// Represents middleware for handling WebSocket commands.
    /// </summary>
    /// <typeparam name="TKey">The type of the command key.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class WebSocketCommandMiddleware<TKey, TPackageInfo> : CommandMiddleware<TKey, WebSocketPackage, TPackageInfo>, IWebSocketCommandMiddleware
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandMiddleware{TKey, TPackageInfo}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="commandOptions">The command options.</param>
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandMiddleware{TKey, TPackageInfo}"/> class with a package mapper.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="commandOptions">The command options.</param>
        /// <param name="mapper">The package mapper.</param>
        public WebSocketCommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions, IPackageMapper<WebSocketPackage, TPackageInfo> mapper)
            : base(serviceProvider, commandOptions, mapper)
        {

        }
    }
}