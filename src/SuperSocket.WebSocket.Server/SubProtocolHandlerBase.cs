using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    abstract class SubProtocolHandlerBase : ISubProtocolHandler
    {
        public string Name { get; private set; }

        public SubProtocolHandlerBase(string name)
        {
            Name = name;
        }

        public abstract ValueTask Handle(IAppSession session, WebSocketPackage package);
    }
}