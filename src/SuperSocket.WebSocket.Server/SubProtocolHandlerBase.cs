using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Serves as a base class for WebSocket sub-protocol handlers.
    /// </summary>
    abstract class SubProtocolHandlerBase : ISubProtocolHandler
    {
        /// <summary>
        /// Gets the name of the sub-protocol.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubProtocolHandlerBase"/> class.
        /// </summary>
        /// <param name="name">The name of the sub-protocol.</param>
        public SubProtocolHandlerBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Handles a WebSocket package for the sub-protocol.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The WebSocket package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public abstract ValueTask Handle(IAppSession session, WebSocketPackage package, CancellationToken cancellationToken);
    }
}