using System;
using System.Collections.Generic;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.OpHandlers
{
    class BinaryHandler : OpHandlerBase
    {
        public BinaryHandler(WebSocketServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override sbyte OpCode
        {
            get { return SuperSocket.WebSocket.OpCode.Binary; }
        }

        public override WebSocketPackageInfo Handle(IAppSession session, WebSocketContext context, IList<ArraySegment<byte>> data)
        {
            return new WebSocketPackageInfo(data, ServiceProvider.BinaryDataParser, ServiceProvider.StringParser);
        }
    }
}
