using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    interface ISubProtocolHandler : IPackageHandler<WebSocketPackage>
    {
        string Name { get; }
    }
}