using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server.Abstractions;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Defines a handler for WebSocket sub-protocols.
    /// </summary>
    interface ISubProtocolHandler : IPackageHandler<WebSocketPackage>
    {
        /// <summary>
        /// Gets the name of the sub-protocol.
        /// </summary>
        string Name { get; }
    }
}