using System;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

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