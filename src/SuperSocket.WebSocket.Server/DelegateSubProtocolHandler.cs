using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    class DelegateSubProtocolHandler : SubProtocolHandlerBase
    {
        private Func<WebSocketSession, WebSocketPackage, ValueTask> _packageHandler;

        public DelegateSubProtocolHandler(string name, Func<WebSocketSession, WebSocketPackage, ValueTask> packageHandler)
            : base(name)
        {
            _packageHandler = packageHandler;
        }

        public override async ValueTask Handle(IAppSession session, WebSocketPackage package)
        {
            await _packageHandler(session as WebSocketSession, package);
        }
    }
}