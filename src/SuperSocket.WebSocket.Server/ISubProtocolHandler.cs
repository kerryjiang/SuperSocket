using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    interface ISubProtocolHandler : IPackageHandler<WebSocketPackage>
    {
        string Name { get; }
    }
}